## FEATURE:

Common requirements:
 - Use messaging service RabbitMq. Update docker compose file so this service can be created inside docker.
 - Add event bus into the project so the messages/events can be sent/published. As the transport system for event bus use the chosen messaging service.

Order creation handling:
 - When the order is created the OrderCreated event has to be published
 - There will be handling of this event which:
   - Update order status: pending -> processing
   - Simulate payment processing (5 second delay)
   - Update order status for 50% of cases to completed and publish OrderCompleted event
   - In another 50% of cases do not change the status

Order expiration handling:
 - Add recursive job which will run every 60 seconds
 - The job find orders with status='processing' older than 10 minutes and update the status to 'expired'
 - Publish OrderExpired event

Notifications handling
 - Create new notifications table and add upgrade script/code
 - When the OrderCompleted event is published
   - Send email notification (for now mock it just log to console)
   - Save notification to database (audit trail)
 - When the OrderExpired event is published
   - Save notification to database (audit trail)

Expected Flow:
1. User creates order via POST /api/orders
2. Order saved to DB with status='pending'
3. OrderCreated event published
4. OrderProcessor handles event asynchronously:
  - Updates status to 'processing'
  - Simulates payment (5 sec delay)
  - Updates status to 'completed'
5. OrderCompleted event published
6. Notifier handles event:
  - Logs fake email to console
  - Saves notification to DB
7. CRON job runs every 60s:
  - Finds pending orders in status processing older than 10 minutes
  - Updates them to 'expired'

Additional requirements
 - Add tests for added functionality
 - all ids are GUIDs
 - every date is in UTC

## EXAMPLES:

### Relevant Codebase References

**Order Entity and Status Enum:**
- Order entity: `WebApi/Orders/Domain/Order.cs:1-12`
  - Already has `Status` property of type `OrderStatus` at line 8
  - Uses UTC dates: `CreatedAt` and `UpdatedAt` properties (lines 9-10)
  - Uses GUID for `Id` (line 5)
- OrderStatus enum: `WebApi/Orders/Domain/OrderStatus.cs:1-9`
  - Already defines: `Pending`, `Processing`, `Completed`, `Expired` (lines 5-8)
  - Configured with `.HasConversion<string>()` in `WebApi/Dal/Configurations/OrderConfiguration.cs:30`

**Order Creation Handler:**
- CreateOrderHandler: `WebApi/Orders/Features/CreateOrder/CreateOrderHandler.cs:9-75`
  - Sets initial status to `OrderStatus.Pending` at line 49
  - Uses `DateTime.UtcNow` for timestamps (lines 50-51)
  - Uses `Guid.NewGuid()` for ID generation (line 46)
  - Returns `Results.Created()` at line 73
  - **NOTE:** Event publishing needs to be added here after order is saved

**Database Context:**
- OrderProcessingDbContext: `WebApi/Dal/OrderProcessingDbContext.cs:1-102`
  - DbSet for Orders at line 17
  - Seed data uses fixed GUIDs and UTC dates (lines 34-100)
  - Configurations applied from assembly at line 25
  - **NOTE:** Will need to add DbSet for Notifications table

**Entity Configuration Pattern:**
- OrderConfiguration: `WebApi/Dal/Configurations/OrderConfiguration.cs:1-46`
  - GUID primary keys use `.ValueGeneratedNever()` (line 17)
  - Enum stored as string with `.HasConversion<string>()` (line 30)
  - DateTime fields are required (line 35)
  - **NOTE:** Follow this pattern for NotificationConfiguration

**Service Registration:**
- Program.cs: `WebApi/Program.cs:1-123`
  - DbContext registered at lines 18-20
  - MediatR with behaviors at lines 22-27
  - **NOTE:** Event bus, background jobs, and RabbitMQ services will be registered here

**Docker Compose:**
- docker-compose.yml: `docker-compose.yml:1-22`
  - Currently has PostgreSQL service (lines 4-19)
  - Uses version '3.8' (line 1)
  - **NOTE:** Add RabbitMQ service with management plugin here

**Testing Pattern:**
- Handler tests use in-memory database (see CLAUDE.md:309-320)
- Tests use xUnit, FluentAssertions, and NSubstitute for mocks
- InMemoryDbHelper: `Tests/WebApi.Tests/TestHelpers/InMemoryDbHelper.cs:1-18`
  - Creates unique in-memory database per test (line 11)
  - Returns configured OrderProcessingDbContext (line 14)
- Test example: `Tests/WebApi.Tests/CreateUserHandlerTests.cs:1-50`
  - Uses InMemoryDbHelper.CreateDbContext() pattern (line 17)
  - Follows Arrange-Act-Assert pattern
  - Uses FluentAssertions for verification (lines 25, 29-35)
- **NOTE:** Event handlers and background jobs will need unit tests with mocked dependencies

### Vertical Slice Architecture Pattern

**For Event Consumers (MassTransit pattern), follow this structure:**
```
Orders/
├── Events/              # Event record classes (immutable)
│   ├── OrderCreatedEvent.cs      # record OrderCreatedEvent(Guid OrderId, ...)
│   ├── OrderCompletedEvent.cs
│   └── OrderExpiredEvent.cs
├── EventConsumers/      # Consumer classes (implement IConsumer<T>)
│   ├── OrderCreatedConsumer.cs   # Handles OrderCreatedEvent
│   ├── OrderCompletedConsumer.cs
│   └── OrderExpiredConsumer.cs
```
**Note:** Use "Consumer" suffix (not "Handler") for MassTransit IConsumer implementations.
Event classes should be immutable records following DTO pattern (see `WebApi/Orders/DTOs/OrderResponse.cs:1-10`).

**For Notifications module:**
```
Notifications/
├── Domain/            # Notification entity
├── DTOs/             # NotificationResponse (if needed for API endpoints)
└── EventConsumers/   # Consumers for saving notifications
    ├── OrderCompletedConsumer.cs  # Handles OrderCompletedEvent → saves notification
    └── OrderExpiredConsumer.cs    # Handles OrderExpiredEvent → saves notification
```
**Note:** Notification consumers react to order events and persist audit trail to database.

**For Background Jobs:**
```
Jobs/
├── OrderExpirationJob.cs  # Background service implementing IHostedService
```

**Common/Infrastructure (if needed):**
```
Common/
└── Configuration/  # RabbitMqSettings (optional - can use appsettings.json directly)
```
**Note:** MassTransit provides built-in `IPublishEndpoint` interface, so no need to create custom IEventBus abstraction.
Configuration will be added directly to `appsettings.json` under "RabbitMqSettings" section.

### Key Architectural Notes

1. **MassTransit for Event-Driven Architecture**: Use MassTransit with RabbitMQ transport
   - Events are published via `IPublishEndpoint.Publish()` method
   - Consumers implement `IConsumer<TEvent>` interface with `Consume(ConsumeContext<TEvent>)` method
   - Multiple consumers can subscribe to the same event type
   - MassTransit automatically handles message serialization, queue creation, and retry policies
   - Example: `await _publishEndpoint.Publish(new OrderCreatedEvent(...), cancellationToken);`
   - **Alternative Note**: Project already uses MediatR (see CLAUDE.md:53-61) for CQRS commands/queries,
     but for distributed messaging with RabbitMQ, MassTransit is the recommended approach

2. **Background Jobs**: Use `IHostedService` or `BackgroundService` for recurring jobs
   - Register in Program.cs with `builder.Services.AddHostedService<OrderExpirationJob>()`
   - Use IServiceScopeFactory to create scopes for DbContext access (required because BackgroundService is singleton)
   - Example pattern: `using var scope = _serviceScopeFactory.CreateScope(); var context = scope.ServiceProvider.GetRequiredService<OrderProcessingDbContext>();`

3. **Event Publishing Pattern**:
   - Inject `IPublishEndpoint` into handlers/consumers (not IEventBus abstraction)
   - MassTransit provides IPublishEndpoint for publishing messages to RabbitMQ
   - Events are immutable records (e.g., `record OrderCreatedEvent(Guid OrderId, Guid UserId, decimal Total)`)
   - Events serialized to JSON and sent to RabbitMQ exchange

4. **UTC Dates**: All DateTime properties must use `DateTime.UtcNow` (see CLAUDE.md:302)

5. **GUID IDs**: All entities use `Guid.NewGuid()` for ID generation (see CLAUDE.md:301)

6. **Testing with Background Services**: Mock IEventBus and use in-memory database

## DOCUMENTATION:

### External Libraries to Add

**RabbitMQ/MassTransit:**
- MassTransit.RabbitMQ - Modern .NET messaging framework
- Recommended over raw RabbitMQ.Client for better abstractions
- Documentation: https://masstransit.io/documentation/concepts

**Background Jobs:**
- Use built-in `IHostedService` / `BackgroundService` from Microsoft.Extensions.Hosting
- For more advanced scheduling, consider Hangfire or Quartz.NET
- Documentation: https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services

**Testing:**
- NSubstitute for mocking: https://nsubstitute.github.io/
- Already have xUnit and FluentAssertions (see CLAUDE.md:227)

### Internal Documentation

- **CLAUDE.md** - Project architecture and conventions (lines 1-321)
  - Vertical Slice Architecture (lines 29-51)
  - CQRS with MediatR (lines 53-61)
  - Entity Framework Configurations (lines 250-258)
  - Testing Approach (lines 309-320)
  - Common Gotchas (lines 298-307)

- **Adding New Features** pattern (CLAUDE.md:285-296):
  1. Create Domain Entity
  2. Create DTOs
  3. Create Commands/Queries/Events
  4. Create Validators
  5. Create Handlers
  6. Create Endpoints (if needed)
  7. Register in Program.cs
  8. Create Entity Configuration
  9. Create Migration
  10. Create Tests

### RabbitMQ Docker Setup

- Add to docker-compose.yml alongside PostgreSQL
- Use official `rabbitmq:3-management` image for admin UI
- Expose port 5672 (AMQP) and 15672 (Management UI)
- Set default credentials via environment variables
- Add health check using `rabbitmq-diagnostics ping`

## OTHER CONSIDERATIONS:

 - follow the existing structure and domain driven design
 - test will be implemented using xUnit and FluentAssertions and if needed for moqs use NSubstitute
 - for tests use in memory database provider for EF Core
 - focus on clean code and SOLID principles, YAGNI principle, KISS principle and functional requirements
 - update CLAUDE.md file with any new information you find important

### Specific Implementation Notes

**Event Publishing in CreateOrderHandler:**
- After `_context.SaveChangesAsync()` at line 61 of CreateOrderHandler.cs
- Inject `IPublishEndpoint` from MassTransit into constructor
- Publish OrderCreated event: `await _publishEndpoint.Publish(new OrderCreatedEvent(...), cancellationToken);`
- Event should contain OrderId, UserId, Total, and CreatedAt (key order properties)
- Example modification:
  ```csharp
  // Add to constructor
  private readonly IPublishEndpoint _publishEndpoint;

  public CreateOrderHandler(OrderProcessingDbContext context, IPublishEndpoint publishEndpoint)
  {
      _context = context;
      _publishEndpoint = publishEndpoint;
  }

  // After line 61 (after SaveChangesAsync)
  await _publishEndpoint.Publish(new OrderCreatedEvent(
      order.Id,
      order.UserId,
      order.Total,
      order.CreatedAt
  ), cancellationToken);
  ```

**Order Status Transitions:**
- Current: Order created with `Status = OrderStatus.Pending` (CreateOrderHandler.cs:49)
- Transition 1: Pending → Processing (in OrderCreatedConsumer)
- Transition 2a: Processing → Completed (in OrderCreatedConsumer after 5s delay, 50% chance)
- Transition 2b: Processing → Expired (in OrderExpirationJob, if older than 10 minutes and still in Processing)

**Notification Entity:**
- Add to new `Notifications/Domain/` directory
- Properties: Id (Guid), OrderId (Guid), Type (enum: OrderCompleted, OrderExpired), Message (string), CreatedAt (DateTime UTC)
- Add NotificationConfiguration following pattern in Dal/Configurations/OrderConfiguration.cs
- Add DbSet to OrderProcessingDbContext.cs

**Background Job Implementation:**
- Create `Jobs/OrderExpirationJob.cs` inheriting from `BackgroundService`
- Override `ExecuteAsync` method with while loop and 60-second delay using `Task.Delay(TimeSpan.FromSeconds(60), stoppingToken)`
- Use IServiceScopeFactory to get scoped DbContext (required because BackgroundService is singleton)
- Example pattern:
  ```csharp
  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
      while (!stoppingToken.IsCancellationRequested)
      {
          using var scope = _serviceScopeFactory.CreateScope();
          var context = scope.ServiceProvider.GetRequiredService<OrderProcessingDbContext>();
          var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

          // Find expired orders
          var expirationCutoff = DateTime.UtcNow.AddMinutes(-10);
          var expiredOrders = await context.Orders
              .Where(o => o.Status == OrderStatus.Processing && o.UpdatedAt < expirationCutoff)
              .ToListAsync(stoppingToken);

          // Update status and publish events
          foreach (var order in expiredOrders)
          {
              order.Status = OrderStatus.Expired;
              order.UpdatedAt = DateTime.UtcNow;
              await publishEndpoint.Publish(new OrderExpiredEvent(...), stoppingToken);
          }

          await context.SaveChangesAsync(stoppingToken);
          await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
      }
  }
  ```

**Testing Strategy:**
- Unit test event handlers with mocked dependencies (using NSubstitute)
- Follow existing handler test pattern from `Tests/WebApi.Tests/CreateUserHandlerTests.cs`:
  - Use InMemoryDbHelper.CreateDbContext() to create test database (line 17)
  - Inject handler dependencies directly in test constructor
  - Use NSubstitute's `Substitute.For<T>()` for mocking interfaces
  - Verify database changes with EF Core queries (lines 28-35)
  - Use FluentAssertions for assertions (e.g., `.Should().NotBeNull()`, `.Should().Be()`)
- Mock IPublishEndpoint in tests to avoid RabbitMQ dependency
- Test background job by calling ExecuteAsync directly with cancellation token
- Example test structure:
  ```csharp
  // Arrange
  var context = InMemoryDbHelper.CreateDbContext();
  var publishEndpoint = Substitute.For<IPublishEndpoint>();
  var consumer = new OrderCreatedConsumer(context, publishEndpoint, logger);

  // Act
  await consumer.Consume(consumeContext);

  // Assert
  var order = await context.Orders.FindAsync(orderId);
  order.Status.Should().Be(OrderStatus.Processing);
  ```

**Migration Notes:**
- After adding Notification entity, run: `dotnet ef migrations add AddNotifications --project WebApi/WebApi.csproj`
- Seed data for Notifications table should use fixed GUIDs and UTC dates (see CLAUDE.md:280-281)
- Apply migration: `dotnet ef database update --project WebApi/WebApi.csproj`

**Error Handling:**
- Event handlers should catch exceptions and log errors (don't crash the app)
- Background jobs should use try-catch and continue running on errors
- Failed events could be retried using MassTransit retry policies
