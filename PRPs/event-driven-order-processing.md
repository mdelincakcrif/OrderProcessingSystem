# COMPREHENSIVE PRODUCT REQUIREMENTS & PLAN (PRP)
## Event-Driven Order Processing with RabbitMQ and MassTransit

---

## EXECUTIVE SUMMARY

This PRP provides a complete, one-pass implementation guide for adding event-driven architecture to the existing Order Processing System using RabbitMQ, MassTransit, background jobs, and notifications. The plan builds upon the established vertical slice architecture and CQRS patterns, adding asynchronous order processing, automated expiration, and comprehensive notification tracking.

**Confidence Score: 8.5/10** - High confidence for one-pass implementation success due to:
- Clear feature requirements with specific business rules
- Existing codebase with established patterns to follow
- Well-documented MassTransit integration patterns
- Comprehensive external documentation and examples
- Validation gates at each phase
- Known gotchas documented and mitigated

**Risks reducing score from 10 to 8.5:**
- **Minor Risk (0.5 points):** RabbitMQ integration requires external service coordination (Docker container must be running)
- **Minor Risk (0.5 points):** Background job timing for testing requires time-based assertions or mocking
- **Minor Risk (0.5 points):** Multiple consumers handling same event requires careful testing for race conditions

---

## 1. FEATURE OVERVIEW

### 1.1 Business Requirements

**Event-Driven Order Processing:**
- When order is created, publish `OrderCreatedEvent`
- Event consumer processes order asynchronously:
  - Update status: Pending → Processing
  - Simulate payment (5-second delay)
  - 50% success rate: Processing → Completed (publish `OrderCompletedEvent`)
  - 50% remain in Processing state (no status change)

**Order Expiration Management:**
- Background job runs every 60 seconds
- Find orders with status='Processing' older than 10 minutes
- Update status to 'Expired'
- Publish `OrderExpiredEvent` for each expired order

**Notification Audit Trail:**
- New Notifications table tracks all order events
- When `OrderCompletedEvent` published:
  - Log email notification to console (mocked)
  - Save notification record to database
- When `OrderExpiredEvent` published:
  - Save notification record to database (no email)

### 1.2 Expected Flow

```
1. User creates order via POST /api/orders
2. Order saved to DB with status='Pending'
3. OrderCreatedEvent published to RabbitMQ
4. OrderCreatedConsumer handles event asynchronously:
   - Updates status to 'Processing'
   - Simulates payment (5 sec delay)
   - 50% chance: Updates status to 'Completed' + publishes OrderCompletedEvent
   - 50% chance: Remains 'Processing'
5. OrderCompletedConsumer handles event:
   - Logs fake email to console
   - Saves notification to DB
6. OrderExpirationJob runs every 60s:
   - Finds Processing orders older than 10 minutes
   - Updates them to 'Expired'
   - Publishes OrderExpiredEvent for each
7. OrderExpiredConsumer handles event:
   - Saves notification to DB
```

---

## 2. PROJECT STRUCTURE ADDITIONS

### 2.1 New Folder Hierarchy

```
C:\Projects\OrderProcessingSystem\
├── docker-compose.yml                           # UPDATED: Add RabbitMQ service
├── CLAUDE.md                                     # UPDATED: Document event-driven patterns
├── WebApi/
│   ├── WebApi.csproj                            # UPDATED: Add MassTransit packages
│   ├── Program.cs                               # UPDATED: Register MassTransit, consumers, jobs
│   ├── appsettings.json                         # UPDATED: Add RabbitMQ settings
│   ├── appsettings.Development.json             # UPDATED: Development RabbitMQ config
│   ├── Dal/
│   │   ├── OrderProcessingDbContext.cs          # UPDATED: Add Notifications DbSet
│   │   ├── Migrations/                          # NEW: AddNotifications migration
│   │   └── Configurations/
│   │       └── NotificationConfiguration.cs     # NEW: Notification entity config
│   ├── Jobs/                                    # NEW MODULE
│   │   └── OrderExpirationJob.cs                # Background service for order expiration
│   ├── Notifications/                           # NEW MODULE
│   │   ├── Domain/
│   │   │   ├── Notification.cs                  # Notification entity
│   │   │   └── NotificationType.cs              # Enum: OrderCompleted, OrderExpired
│   │   └── EventConsumers/
│   │       ├── OrderCompletedNotificationConsumer.cs
│   │       └── OrderExpiredNotificationConsumer.cs
│   └── Orders/
│       ├── Events/                              # NEW: Event messages
│       │   ├── OrderCreatedEvent.cs
│       │   ├── OrderCompletedEvent.cs
│       │   └── OrderExpiredEvent.cs
│       ├── EventConsumers/                      # NEW: Event handlers
│       │   └── OrderCreatedConsumer.cs
│       └── Features/
│           └── CreateOrder/
│               └── CreateOrderHandler.cs        # UPDATED: Publish OrderCreatedEvent
└── Tests/
    └── WebApi.Tests/
        ├── WebApi.Tests.csproj                  # UPDATED: Add NSubstitute if needed
        ├── OrderCreatedConsumerTests.cs         # NEW: Consumer unit tests
        ├── OrderCompletedNotificationConsumerTests.cs # NEW
        ├── OrderExpiredNotificationConsumerTests.cs  # NEW
        └── OrderExpirationJobTests.cs           # NEW: Background job tests
```

### 2.2 Key Changes to Existing Files

**Modified Files:**
1. `docker-compose.yml` - Add RabbitMQ service
2. `WebApi/WebApi.csproj` - Add MassTransit NuGet packages
3. `WebApi/Program.cs` - Register MassTransit, consumers, background jobs
4. `WebApi/appsettings.json` - Add RabbitMQ connection settings
5. `WebApi/Dal/OrderProcessingDbContext.cs` - Add Notifications DbSet
6. `WebApi/Orders/Features/CreateOrder/CreateOrderHandler.cs` - Publish OrderCreatedEvent
7. `CLAUDE.md` - Document event-driven patterns

**New Files:** 21 files total
- 3 Event records (OrderCreatedEvent, OrderCompletedEvent, OrderExpiredEvent)
- 1 Event consumer (OrderCreatedConsumer)
- 2 Notification entities (Notification.cs, NotificationType.cs)
- 2 Notification consumers (OrderCompletedNotificationConsumer, OrderExpiredNotificationConsumer)
- 1 Background job (OrderExpirationJob.cs)
- 1 Entity configuration (NotificationConfiguration.cs)
- 4 Test files
- 1 Migration file (AddNotifications)
- 6 Additional supporting files

---

## 3. NUGET PACKAGES

### 3.1 New Packages for WebApi Project

Add these packages to `C:\Projects\OrderProcessingSystem\WebApi\WebApi.csproj`:

```xml
<PackageReference Include="MassTransit" Version="8.5.2" />
<PackageReference Include="MassTransit.RabbitMQ" Version="8.5.2" />
```

**Commands to add packages:**
```bash
dotnet add WebApi/WebApi.csproj package MassTransit --version 8.5.2
dotnet add WebApi/WebApi.csproj package MassTransit.RabbitMQ --version 8.5.2
```

### 3.2 Test Project Packages

NSubstitute is already included in the test project for mocking `IPublishEndpoint`.

---

## 4. IMPLEMENTATION ORDER

### Phase 1: RabbitMQ Infrastructure (Validation: docker-compose up succeeds)

**Tasks:**
1. Update `docker-compose.yml` to add RabbitMQ service with management plugin
2. Update `appsettings.json` with RabbitMQ connection settings
3. Start RabbitMQ container and verify management UI is accessible

**Validation Gate 1:**
```bash
docker-compose up -d
# Navigate to http://localhost:15672
# Login with guest/guest (default credentials)
```
Expected: RabbitMQ management UI accessible, no errors in container logs

---

### Phase 2: MassTransit Setup (Validation: dotnet build succeeds)

**Tasks:**
4. Add MassTransit NuGet packages to WebApi project
5. Add RabbitMQ configuration section to `appsettings.json`
6. Register MassTransit with RabbitMQ transport in `Program.cs`
7. Configure MassTransit to auto-discover consumers

**Validation Gate 2:**
```bash
dotnet build WebApi/WebApi.csproj
```
Expected: Build succeeds with no errors

---

### Phase 3: Event Messages (Validation: dotnet build succeeds)

**Tasks:**
8. Create `Orders/Events/OrderCreatedEvent.cs` record
9. Create `Orders/Events/OrderCompletedEvent.cs` record
10. Create `Orders/Events/OrderExpiredEvent.cs` record

**Validation Gate 3:**
```bash
dotnet build WebApi/WebApi.csproj
```
Expected: Build succeeds with no errors

---

### Phase 4: Order Event Publishing (Validation: dotnet build succeeds)

**Tasks:**
11. Update `CreateOrderHandler.cs` to inject `IPublishEndpoint`
12. Publish `OrderCreatedEvent` after saving order to database
13. Test event publishing by running application and checking RabbitMQ management UI

**Validation Gate 4:**
```bash
dotnet build WebApi/WebApi.csproj
dotnet run --project WebApi/WebApi.csproj
# Create an order via Swagger
# Check RabbitMQ management UI for published messages
```
Expected: Build succeeds, application runs, events appear in RabbitMQ queues

---

### Phase 5: Order Processing Consumer (Validation: dotnet build succeeds, tests pass)

**Tasks:**
14. Create `Orders/EventConsumers/OrderCreatedConsumer.cs`
15. Implement logic: update status to Processing, simulate payment, 50% completion rate
16. Publish `OrderCompletedEvent` for successful orders
17. Create unit test `OrderCreatedConsumerTests.cs`

**Validation Gate 5:**
```bash
dotnet build WebApi/WebApi.csproj
dotnet test Tests/WebApi.Tests/WebApi.Tests.csproj --filter "OrderCreatedConsumerTests"
```
Expected: Build succeeds, consumer tests pass

---

### Phase 6: Notifications Module (Validation: migration created, dotnet build succeeds)

**Tasks:**
18. Create `Notifications/Domain/Notification.cs` entity
19. Create `Notifications/Domain/NotificationType.cs` enum
20. Create `Dal/Configurations/NotificationConfiguration.cs`
21. Update `OrderProcessingDbContext.cs` to add Notifications DbSet
22. Create migration: `dotnet ef migrations add AddNotifications`
23. Apply migration: `dotnet ef database update`

**Validation Gate 6:**
```bash
dotnet ef migrations add AddNotifications --project WebApi/WebApi.csproj
dotnet ef database update --project WebApi/WebApi.csproj
dotnet build WebApi/WebApi.csproj
```
Expected: Migration created and applied successfully, build succeeds

---

### Phase 7: Notification Consumers (Validation: dotnet build succeeds, tests pass)

**Tasks:**
24. Create `Notifications/EventConsumers/OrderCompletedNotificationConsumer.cs`
25. Implement logic: log email to console, save notification to DB
26. Create `Notifications/EventConsumers/OrderExpiredNotificationConsumer.cs`
27. Implement logic: save notification to DB
28. Create tests: `OrderCompletedNotificationConsumerTests.cs`
29. Create tests: `OrderExpiredNotificationConsumerTests.cs`

**Validation Gate 7:**
```bash
dotnet build WebApi/WebApi.csproj
dotnet test Tests/WebApi.Tests/WebApi.Tests.csproj --filter "OrderCompletedNotificationConsumerTests"
dotnet test Tests/WebApi.Tests/WebApi.Tests.csproj --filter "OrderExpiredNotificationConsumerTests"
```
Expected: Build succeeds, all consumer tests pass

---

### Phase 8: Background Job for Order Expiration (Validation: dotnet build succeeds, tests pass)

**Tasks:**
30. Create `Jobs/OrderExpirationJob.cs` inheriting from `BackgroundService`
31. Implement logic: run every 60 seconds, find expired orders, update status, publish events
32. Register job in `Program.cs` with `AddHostedService<OrderExpirationJob>()`
33. Create tests: `OrderExpirationJobTests.cs`

**Validation Gate 8:**
```bash
dotnet build WebApi/WebApi.csproj
dotnet test Tests/WebApi.Tests/WebApi.Tests.csproj --filter "OrderExpirationJobTests"
```
Expected: Build succeeds, background job tests pass

---

### Phase 9: Integration Testing (Validation: dotnet test succeeds, end-to-end flow works)

**Tasks:**
34. Run complete application with RabbitMQ
35. Test end-to-end flow:
    - Create order via Swagger
    - Verify order status changes to Processing
    - Verify 50% complete successfully
    - Verify notifications are saved
    - Wait 10+ minutes, verify expiration job works
36. Run all tests to ensure no regressions

**Validation Gate 9:**
```bash
docker-compose up -d
dotnet run --project WebApi/WebApi.csproj
# Manual testing via Swagger
dotnet test
```
Expected: All tests pass, end-to-end flow works correctly

---

### Phase 10: Documentation & Cleanup (Validation: manual review)

**Tasks:**
37. Update CLAUDE.md with event-driven architecture patterns
38. Document RabbitMQ setup and usage
39. Document background job patterns
40. Final verification of all functionality

**Validation Gate 10:**
```bash
dotnet build
dotnet test
# Manual review of CLAUDE.md updates
```
Expected: All tests pass, documentation is complete and accurate

---

## 5. CODE PATTERNS

### 5.1 Event Message Pattern

**File:** `C:\Projects\OrderProcessingSystem\WebApi\Orders\Events\OrderCreatedEvent.cs`

```csharp
namespace WebApi.Orders.Events;

public record OrderCreatedEvent(
    Guid OrderId,
    Guid UserId,
    decimal Total,
    DateTime CreatedAt
);
```

**File:** `C:\Projects\OrderProcessingSystem\WebApi\Orders\Events\OrderCompletedEvent.cs`

```csharp
namespace WebApi.Orders.Events;

public record OrderCompletedEvent(
    Guid OrderId,
    Guid UserId,
    decimal Total,
    DateTime CompletedAt
);
```

**File:** `C:\Projects\OrderProcessingSystem\WebApi\Orders\Events\OrderExpiredEvent.cs`

```csharp
namespace WebApi.Orders.Events;

public record OrderExpiredEvent(
    Guid OrderId,
    Guid UserId,
    DateTime ExpiredAt
);
```

**Key Points:**
- Events are immutable records (matches DTO pattern in codebase)
- Include all essential order information (OrderId, UserId, timestamps)
- Use descriptive past-tense names (OrderCreated, OrderCompleted, OrderExpired)

---

### 5.2 Event Publishing Pattern (Updated CreateOrderHandler)

**File:** `C:\Projects\OrderProcessingSystem\WebApi\Orders\Features\CreateOrder\CreateOrderHandler.cs`

**Changes to apply:**

```csharp
using MassTransit;  // ADD THIS
using WebApi.Orders.Events;  // ADD THIS

public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, IResult>
{
    private readonly OrderProcessingDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;  // ADD THIS

    public CreateOrderHandler(
        OrderProcessingDbContext context,
        IPublishEndpoint publishEndpoint)  // ADD THIS PARAMETER
    {
        _context = context;
        _publishEndpoint = publishEndpoint;  // ADD THIS
    }

    public async Task<IResult> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // ... existing validation and order creation code (lines 20-60) ...

        _context.Orders.Add(order);
        await _context.SaveChangesAsync(cancellationToken);

        // ADD THIS: Publish OrderCreatedEvent after successful save
        await _publishEndpoint.Publish(new OrderCreatedEvent(
            order.Id,
            order.UserId,
            order.Total,
            order.CreatedAt
        ), cancellationToken);

        var response = new OrderResponse(/* ... existing code ... */);
        return Results.Created($"/api/orders/{order.Id}", response);
    }
}
```

**Key Points:**
- Inject `IPublishEndpoint` from MassTransit (not custom IEventBus)
- Publish event AFTER `SaveChangesAsync` to ensure DB consistency
- Include cancellationToken for proper async cancellation support

---

### 5.3 Event Consumer Pattern

**File:** `C:\Projects\OrderProcessingSystem\WebApi\Orders\EventConsumers\OrderCreatedConsumer.cs`

```csharp
using MassTransit;
using Microsoft.EntityFrameworkCore;
using WebApi.Dal;
using WebApi.Orders.Domain;
using WebApi.Orders.Events;

namespace WebApi.Orders.EventConsumers;

public class OrderCreatedConsumer : IConsumer<OrderCreatedEvent>
{
    private readonly OrderProcessingDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<OrderCreatedConsumer> _logger;

    public OrderCreatedConsumer(
        OrderProcessingDbContext context,
        IPublishEndpoint publishEndpoint,
        ILogger<OrderCreatedConsumer> logger)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Processing OrderCreatedEvent for Order {OrderId}", message.OrderId);

        // Find the order
        var order = await _context.Orders.FindAsync(message.OrderId);
        if (order == null)
        {
            _logger.LogWarning("Order {OrderId} not found", message.OrderId);
            return;
        }

        // Update status to Processing
        order.Status = OrderStatus.Processing;
        order.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Order {OrderId} status updated to Processing", message.OrderId);

        // Simulate payment processing (5 second delay)
        await Task.Delay(TimeSpan.FromSeconds(5), context.CancellationToken);

        // 50% success rate
        var random = new Random();
        var isSuccessful = random.Next(0, 2) == 1;

        if (isSuccessful)
        {
            order.Status = OrderStatus.Completed;
            order.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Order {OrderId} completed successfully", message.OrderId);

            // Publish OrderCompletedEvent
            await _publishEndpoint.Publish(new OrderCompletedEvent(
                order.Id,
                order.UserId,
                order.Total,
                order.UpdatedAt
            ), context.CancellationToken);
        }
        else
        {
            _logger.LogInformation("Order {OrderId} remains in Processing state (payment pending)", message.OrderId);
        }
    }
}
```

**Key Points:**
- Implement `IConsumer<TEvent>` interface from MassTransit
- Use `ConsumeContext<TEvent>` to access message and metadata
- Inject scoped dependencies (DbContext, IPublishEndpoint, ILogger)
- Use `context.CancellationToken` for cancellation support
- Log all important state changes for debugging
- Handle missing orders gracefully (defensive programming)

---

### 5.4 Notification Domain Pattern

**File:** `C:\Projects\OrderProcessingSystem\WebApi\Notifications\Domain\NotificationType.cs`

```csharp
namespace WebApi.Notifications.Domain;

public enum NotificationType
{
    OrderCompleted,
    OrderExpired
}
```

**File:** `C:\Projects\OrderProcessingSystem\WebApi\Notifications\Domain\Notification.cs`

```csharp
namespace WebApi.Notifications.Domain;

public class Notification
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public NotificationType Type { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
```

**File:** `C:\Projects\OrderProcessingSystem\WebApi\Dal\Configurations\NotificationConfiguration.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApi.Notifications.Domain;

namespace WebApi.Dal.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(n => n.OrderId)
            .HasColumnName("order_id")
            .IsRequired();

        builder.Property(n => n.Type)
            .HasColumnName("type")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(n => n.Message)
            .HasColumnName("message")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(n => n.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();
    }
}
```

**Key Points:**
- Follow existing entity configuration pattern (see OrderConfiguration.cs)
- GUID primary key with `ValueGeneratedNever()`
- Enum stored as string with `HasConversion<string>()`
- All DateTime fields use UTC (enforced by application logic)
- Message field has reasonable max length (500 characters)

---

### 5.5 Notification Consumer Pattern

**File:** `C:\Projects\OrderProcessingSystem\WebApi\Notifications\EventConsumers\OrderCompletedNotificationConsumer.cs`

```csharp
using MassTransit;
using WebApi.Dal;
using WebApi.Notifications.Domain;
using WebApi.Orders.Events;

namespace WebApi.Notifications.EventConsumers;

public class OrderCompletedNotificationConsumer : IConsumer<OrderCompletedEvent>
{
    private readonly OrderProcessingDbContext _context;
    private readonly ILogger<OrderCompletedNotificationConsumer> _logger;

    public OrderCompletedNotificationConsumer(
        OrderProcessingDbContext context,
        ILogger<OrderCompletedNotificationConsumer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCompletedEvent> context)
    {
        var message = context.Message;

        // Mock email notification (just log to console)
        _logger.LogInformation(
            "📧 Sending email notification: Order {OrderId} for User {UserId} has been completed. Total: ${Total}",
            message.OrderId,
            message.UserId,
            message.Total);

        // Save notification to database (audit trail)
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            OrderId = message.OrderId,
            Type = NotificationType.OrderCompleted,
            Message = $"Order {message.OrderId} completed successfully. Total: ${message.Total}",
            CreatedAt = DateTime.UtcNow
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Notification saved to database for Order {OrderId}", message.OrderId);
    }
}
```

**File:** `C:\Projects\OrderProcessingSystem\WebApi\Notifications\EventConsumers\OrderExpiredNotificationConsumer.cs`

```csharp
using MassTransit;
using WebApi.Dal;
using WebApi.Notifications.Domain;
using WebApi.Orders.Events;

namespace WebApi.Notifications.EventConsumers;

public class OrderExpiredNotificationConsumer : IConsumer<OrderExpiredEvent>
{
    private readonly OrderProcessingDbContext _context;
    private readonly ILogger<OrderExpiredNotificationConsumer> _logger;

    public OrderExpiredNotificationConsumer(
        OrderProcessingDbContext context,
        ILogger<OrderExpiredNotificationConsumer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderExpiredEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Order {OrderId} expired and has been marked as expired",
            message.OrderId);

        // Save notification to database (audit trail only, no email)
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            OrderId = message.OrderId,
            Type = NotificationType.OrderExpired,
            Message = $"Order {message.OrderId} expired after 10 minutes in Processing status",
            CreatedAt = DateTime.UtcNow
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Expiration notification saved to database for Order {OrderId}", message.OrderId);
    }
}
```

**Key Points:**
- Use ILogger for console output (structured logging)
- Email notifications are mocked (just logged to console)
- All notifications saved to database for audit trail
- Use `DateTime.UtcNow` for timestamps (project convention)
- Use `Guid.NewGuid()` for ID generation (project convention)

---

### 5.6 Background Service Pattern

**File:** `C:\Projects\OrderProcessingSystem\WebApi\Jobs\OrderExpirationJob.cs`

```csharp
using MassTransit;
using Microsoft.EntityFrameworkCore;
using WebApi.Dal;
using WebApi.Orders.Domain;
using WebApi.Orders.Events;

namespace WebApi.Jobs;

public class OrderExpirationJob : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<OrderExpirationJob> _logger;

    public OrderExpirationJob(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<OrderExpirationJob> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OrderExpirationJob started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessExpiredOrders(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing expired orders");
            }

            // Wait 60 seconds before next run
            await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
        }

        _logger.LogInformation("OrderExpirationJob stopped");
    }

    private async Task ProcessExpiredOrders(CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<OrderProcessingDbContext>();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        // Find orders in Processing status older than 10 minutes
        var expirationCutoff = DateTime.UtcNow.AddMinutes(-10);
        var expiredOrders = await context.Orders
            .Where(o => o.Status == OrderStatus.Processing && o.UpdatedAt < expirationCutoff)
            .ToListAsync(cancellationToken);

        if (expiredOrders.Count == 0)
        {
            _logger.LogDebug("No expired orders found");
            return;
        }

        _logger.LogInformation("Found {Count} expired orders", expiredOrders.Count);

        foreach (var order in expiredOrders)
        {
            order.Status = OrderStatus.Expired;
            order.UpdatedAt = DateTime.UtcNow;

            _logger.LogInformation("Expiring Order {OrderId}", order.Id);

            // Publish OrderExpiredEvent
            await publishEndpoint.Publish(new OrderExpiredEvent(
                order.Id,
                order.UserId,
                order.UpdatedAt
            ), cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Successfully expired {Count} orders", expiredOrders.Count);
    }
}
```

**Key Points:**
- Inherit from `BackgroundService` (built-in .NET class)
- Use `IServiceScopeFactory` to create scopes (required because BackgroundService is singleton)
- Get scoped services (DbContext, IPublishEndpoint) from scope
- Run in infinite loop with 60-second delay
- Wrap processing in try-catch to prevent crashes
- Log all operations for debugging
- Use UTC dates for all time comparisons

---

### 5.7 DbContext Update Pattern

**File:** `C:\Projects\OrderProcessingSystem\WebApi\Dal\OrderProcessingDbContext.cs`

**Changes to apply:**

```csharp
using WebApi.Notifications.Domain;  // ADD THIS

public class OrderProcessingDbContext : DbContext
{
    // ... existing DbSets ...

    public DbSet<Notification> Notifications => Set<Notification>();  // ADD THIS

    // ... rest of the code remains the same ...
}
```

**Key Points:**
- Follow existing DbSet pattern
- No changes to OnModelCreating needed (configurations auto-discovered)

---

## 6. CONFIGURATION FILES

### 6.1 docker-compose.yml Updates

**File:** `C:\Projects\OrderProcessingSystem\docker-compose.yml`

**Changes to apply:**

```yaml
version: '3.8'

services:
  postgres:
    image: postgres:17-alpine
    container_name: orderprocessing-postgres
    environment:
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
      POSTGRES_DB: orderprocessing
    ports:
      - "5433:5432"
    volumes:
      - postgres-data:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres"]
      interval: 10s
      timeout: 5s
      retries: 5

  # ADD THIS ENTIRE SERVICE:
  rabbitmq:
    image: rabbitmq:3-management
    container_name: orderprocessing-rabbitmq
    environment:
      RABBITMQ_DEFAULT_USER: guest
      RABBITMQ_DEFAULT_PASS: guest
    ports:
      - "5672:5672"   # AMQP protocol port
      - "15672:15672" # Management UI port
    volumes:
      - rabbitmq-data:/var/lib/rabbitmq
    healthcheck:
      test: ["CMD", "rabbitmq-diagnostics", "ping"]
      interval: 10s
      timeout: 5s
      retries: 5

volumes:
  postgres-data:
  rabbitmq-data:  # ADD THIS
```

**Key Points:**
- Use official `rabbitmq:3-management` image (includes web UI)
- Port 5672 for AMQP protocol (MassTransit uses this)
- Port 15672 for management UI (http://localhost:15672)
- Default credentials: guest/guest (change in production!)
- Health check ensures RabbitMQ is ready before consumers start
- Named volume for data persistence

---

### 6.2 appsettings.json Updates

**File:** `C:\Projects\OrderProcessingSystem\WebApi\appsettings.json`

**Changes to apply:**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5433;Database=orderprocessing;Username=postgres;Password=postgres"
  },
  "JwtSettings": {
    "Secret": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "OrderProcessingAPI",
    "Audience": "OrderProcessingClient",
    "ExpirationMinutes": 60
  },
  "RabbitMqSettings": {
    "Host": "localhost",
    "Port": 5672,
    "Username": "guest",
    "Password": "guest"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning",
      "MassTransit": "Information"
    }
  },
  "AllowedHosts": "*"
}
```

**Key Points:**
- Add `RabbitMqSettings` section with connection details
- Add MassTransit logging level (helpful for debugging)
- Keep existing settings unchanged

---

### 6.3 appsettings.Development.json Updates

**File:** `C:\Projects\OrderProcessingSystem\WebApi\appsettings.Development.json`

**Changes to apply:**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5433;Database=orderprocessing;Username=postgres;Password=postgres"
  },
  "RabbitMqSettings": {
    "Host": "localhost",
    "Port": 5672,
    "Username": "guest",
    "Password": "guest"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Information",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information",
      "MassTransit": "Debug"
    }
  }
}
```

**Key Points:**
- Use Debug level for MassTransit in development (more verbose)
- Same RabbitMQ settings as production (Docker container)

---

### 6.4 Program.cs Updates

**File:** `C:\Projects\OrderProcessingSystem\WebApi\Program.cs`

**Changes to apply:**

```csharp
// ADD THESE USING STATEMENTS:
using MassTransit;
using WebApi.Jobs;
using WebApi.Orders.EventConsumers;
using WebApi.Notifications.EventConsumers;

var builder = WebApplication.CreateBuilder(args);

// ... existing DbContext, MediatR, FluentValidation, JWT setup ...

// ADD THIS: Register MassTransit with RabbitMQ
builder.Services.AddMassTransit(x =>
{
    // Register all consumers
    x.AddConsumer<OrderCreatedConsumer>();
    x.AddConsumer<OrderCompletedNotificationConsumer>();
    x.AddConsumer<OrderExpiredNotificationConsumer>();

    // Configure RabbitMQ transport
    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitMqSettings = builder.Configuration.GetSection("RabbitMqSettings");
        cfg.Host(rabbitMqSettings["Host"], h =>
        {
            h.Username(rabbitMqSettings["Username"]);
            h.Password(rabbitMqSettings["Password"]);
        });

        // Auto-configure receive endpoints for all consumers
        cfg.ConfigureEndpoints(context);
    });
});

// ADD THIS: Register Background Job
builder.Services.AddHostedService<OrderExpirationJob>();

// ... rest of the existing code (Swagger, etc.) ...

var app = builder.Build();

// ... existing middleware pipeline ...

app.Run();

public partial class Program { }
```

**Key Points:**
- Register all consumers with `AddConsumer<T>()`
- Use `UsingRabbitMq` to configure transport
- Read RabbitMQ settings from `appsettings.json`
- `ConfigureEndpoints` automatically creates queues for consumers
- Register background job with `AddHostedService<T>()`

---

## 7. TESTING STRATEGY

### 7.1 Consumer Test Pattern

**File:** `C:\Projects\OrderProcessingSystem\Tests\WebApi.Tests\OrderCreatedConsumerTests.cs`

```csharp
using FluentAssertions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using WebApi.Orders.Domain;
using WebApi.Orders.EventConsumers;
using WebApi.Orders.Events;
using WebApi.Tests.TestHelpers;
using Xunit;

namespace WebApi.Tests;

public class OrderCreatedConsumerTests
{
    [Fact]
    public async Task Consume_UpdatesOrderStatusToProcessing()
    {
        // Arrange
        var context = InMemoryDbHelper.CreateDbContext();
        var publishEndpoint = Substitute.For<IPublishEndpoint>();
        var logger = Substitute.For<ILogger<OrderCreatedConsumer>>();
        var consumer = new OrderCreatedConsumer(context, publishEndpoint, logger);

        // Create test order
        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Total = 100m,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Items = new List<OrderItem>()
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var orderCreatedEvent = new OrderCreatedEvent(
            order.Id,
            order.UserId,
            order.Total,
            order.CreatedAt
        );

        var consumeContext = Substitute.For<ConsumeContext<OrderCreatedEvent>>();
        consumeContext.Message.Returns(orderCreatedEvent);
        consumeContext.CancellationToken.Returns(CancellationToken.None);

        // Act
        await consumer.Consume(consumeContext);

        // Assert
        var updatedOrder = await context.Orders.FindAsync(order.Id);
        updatedOrder.Should().NotBeNull();
        updatedOrder!.Status.Should().Be(OrderStatus.Processing);
    }

    [Fact]
    public async Task Consume_PublishesOrderCompletedEvent_WhenSuccessful()
    {
        // Arrange
        var context = InMemoryDbHelper.CreateDbContext();
        var publishEndpoint = Substitute.For<IPublishEndpoint>();
        var logger = Substitute.For<ILogger<OrderCreatedConsumer>>();

        // Note: This test has randomness (50% chance), so we may need to run multiple times
        // or mock Random for deterministic testing
        var consumer = new OrderCreatedConsumer(context, publishEndpoint, logger);

        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Total = 100m,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Items = new List<OrderItem>()
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var orderCreatedEvent = new OrderCreatedEvent(
            order.Id,
            order.UserId,
            order.Total,
            order.CreatedAt
        );

        var consumeContext = Substitute.For<ConsumeContext<OrderCreatedEvent>>();
        consumeContext.Message.Returns(orderCreatedEvent);
        consumeContext.CancellationToken.Returns(CancellationToken.None);

        // Act
        await consumer.Consume(consumeContext);

        // Assert
        var updatedOrder = await context.Orders.FindAsync(order.Id);
        updatedOrder.Should().NotBeNull();

        // Order should be either Processing or Completed (50% chance)
        updatedOrder!.Status.Should().BeOneOf(OrderStatus.Processing, OrderStatus.Completed);

        // If completed, verify event was published
        if (updatedOrder.Status == OrderStatus.Completed)
        {
            await publishEndpoint.Received(1).Publish(
                Arg.Is<OrderCompletedEvent>(e => e.OrderId == order.Id),
                Arg.Any<CancellationToken>());
        }
    }

    [Fact]
    public async Task Consume_HandlesNonExistentOrder_Gracefully()
    {
        // Arrange
        var context = InMemoryDbHelper.CreateDbContext();
        var publishEndpoint = Substitute.For<IPublishEndpoint>();
        var logger = Substitute.For<ILogger<OrderCreatedConsumer>>();
        var consumer = new OrderCreatedConsumer(context, publishEndpoint, logger);

        var nonExistentOrderId = Guid.NewGuid();
        var orderCreatedEvent = new OrderCreatedEvent(
            nonExistentOrderId,
            Guid.NewGuid(),
            100m,
            DateTime.UtcNow
        );

        var consumeContext = Substitute.For<ConsumeContext<OrderCreatedEvent>>();
        consumeContext.Message.Returns(orderCreatedEvent);
        consumeContext.CancellationToken.Returns(CancellationToken.None);

        // Act
        var act = async () => await consumer.Consume(consumeContext);

        // Assert
        await act.Should().NotThrowAsync();
    }
}
```

**Key Points:**
- Use NSubstitute to mock `IPublishEndpoint` and `ConsumeContext`
- Use InMemoryDbHelper for database (existing pattern)
- Test both success and failure paths
- Handle randomness in tests (50% completion rate)
- Use FluentAssertions for readable assertions

---

### 7.2 Notification Consumer Test Pattern

**File:** `C:\Projects\OrderProcessingSystem\Tests\WebApi.Tests\OrderCompletedNotificationConsumerTests.cs`

```csharp
using FluentAssertions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using WebApi.Notifications.Domain;
using WebApi.Notifications.EventConsumers;
using WebApi.Orders.Events;
using WebApi.Tests.TestHelpers;
using Xunit;

namespace WebApi.Tests;

public class OrderCompletedNotificationConsumerTests
{
    [Fact]
    public async Task Consume_CreatesNotificationInDatabase()
    {
        // Arrange
        var context = InMemoryDbHelper.CreateDbContext();
        var logger = Substitute.For<ILogger<OrderCompletedNotificationConsumer>>();
        var consumer = new OrderCompletedNotificationConsumer(context, logger);

        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var orderCompletedEvent = new OrderCompletedEvent(
            orderId,
            userId,
            150.50m,
            DateTime.UtcNow
        );

        var consumeContext = Substitute.For<ConsumeContext<OrderCompletedEvent>>();
        consumeContext.Message.Returns(orderCompletedEvent);

        // Act
        await consumer.Consume(consumeContext);

        // Assert
        var notification = await context.Notifications
            .FirstOrDefaultAsync(n => n.OrderId == orderId);

        notification.Should().NotBeNull();
        notification!.Type.Should().Be(NotificationType.OrderCompleted);
        notification.Message.Should().Contain(orderId.ToString());
        notification.Message.Should().Contain("$150.50");
    }

    [Fact]
    public async Task Consume_LogsEmailNotification()
    {
        // Arrange
        var context = InMemoryDbHelper.CreateDbContext();
        var logger = Substitute.For<ILogger<OrderCompletedNotificationConsumer>>();
        var consumer = new OrderCompletedNotificationConsumer(context, logger);

        var orderCompletedEvent = new OrderCompletedEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            100m,
            DateTime.UtcNow
        );

        var consumeContext = Substitute.For<ConsumeContext<OrderCompletedEvent>>();
        consumeContext.Message.Returns(orderCompletedEvent);

        // Act
        await consumer.Consume(consumeContext);

        // Assert
        // Verify logger was called (email notification mocked as log)
        logger.Received().LogInformation(
            Arg.Is<string>(s => s.Contains("Sending email notification")),
            Arg.Any<object[]>());
    }
}
```

---

### 7.3 Background Job Test Pattern

**File:** `C:\Projects\OrderProcessingSystem\Tests\WebApi.Tests\OrderExpirationJobTests.cs`

```csharp
using FluentAssertions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using WebApi.Dal;
using WebApi.Jobs;
using WebApi.Orders.Domain;
using WebApi.Orders.Events;
using WebApi.Tests.TestHelpers;
using Xunit;

namespace WebApi.Tests;

public class OrderExpirationJobTests
{
    [Fact]
    public async Task ProcessExpiredOrders_ExpiresOldProcessingOrders()
    {
        // Arrange
        var context = InMemoryDbHelper.CreateDbContext();
        var publishEndpoint = Substitute.For<IPublishEndpoint>();
        var logger = Substitute.For<ILogger<OrderExpirationJob>>();

        // Create service scope factory mock
        var serviceProvider = Substitute.For<IServiceProvider>();
        serviceProvider.GetService(typeof(OrderProcessingDbContext)).Returns(context);
        serviceProvider.GetService(typeof(IPublishEndpoint)).Returns(publishEndpoint);

        var scope = Substitute.For<IServiceScope>();
        scope.ServiceProvider.Returns(serviceProvider);

        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        scopeFactory.CreateScope().Returns(scope);

        // Create old order (>10 minutes in Processing)
        var oldOrder = new Order
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Total = 100m,
            Status = OrderStatus.Processing,
            CreatedAt = DateTime.UtcNow.AddMinutes(-15),
            UpdatedAt = DateTime.UtcNow.AddMinutes(-15),
            Items = new List<OrderItem>()
        };

        // Create recent order (should not expire)
        var recentOrder = new Order
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Total = 50m,
            Status = OrderStatus.Processing,
            CreatedAt = DateTime.UtcNow.AddMinutes(-5),
            UpdatedAt = DateTime.UtcNow.AddMinutes(-5),
            Items = new List<OrderItem>()
        };

        context.Orders.AddRange(oldOrder, recentOrder);
        await context.SaveChangesAsync();

        var job = new OrderExpirationJob(scopeFactory, logger);

        // Act
        var cancellationTokenSource = new CancellationTokenSource();
        var executeTask = job.StartAsync(cancellationTokenSource.Token);

        // Wait a bit for processing
        await Task.Delay(100);

        // Cancel the background job
        cancellationTokenSource.Cancel();

        try
        {
            await executeTask;
        }
        catch (OperationCanceledException)
        {
            // Expected when cancelling
        }

        // Assert
        var expiredOrder = await context.Orders.FindAsync(oldOrder.Id);
        expiredOrder.Should().NotBeNull();
        expiredOrder!.Status.Should().Be(OrderStatus.Expired);

        var stillProcessingOrder = await context.Orders.FindAsync(recentOrder.Id);
        stillProcessingOrder.Should().NotBeNull();
        stillProcessingOrder!.Status.Should().Be(OrderStatus.Processing);

        // Verify OrderExpiredEvent was published for expired order
        await publishEndpoint.Received(1).Publish(
            Arg.Is<OrderExpiredEvent>(e => e.OrderId == oldOrder.Id),
            Arg.Any<CancellationToken>());
    }
}
```

**Key Points:**
- Background job tests are more complex (need to mock IServiceScopeFactory)
- Test time-based logic (orders older than 10 minutes)
- Verify only old orders are expired
- Verify events are published for expired orders
- Handle cancellation properly

---

## 8. VALIDATION GATES SUMMARY

| Phase | Validation Command | Expected Result |
|-------|-------------------|----------------|
| 1 | `docker-compose up -d` | RabbitMQ container running, UI accessible at http://localhost:15672 |
| 2 | `dotnet build WebApi/WebApi.csproj` | Build succeeds |
| 3 | `dotnet build WebApi/WebApi.csproj` | Build succeeds |
| 4 | `dotnet run --project WebApi/WebApi.csproj` | App runs, events visible in RabbitMQ UI |
| 5 | `dotnet test --filter "OrderCreatedConsumerTests"` | Consumer tests pass |
| 6 | `dotnet ef migrations add AddNotifications` | Migration created |
| 6 | `dotnet ef database update` | Migration applied |
| 7 | `dotnet test --filter "Notification"` | Notification consumer tests pass |
| 8 | `dotnet test --filter "OrderExpirationJobTests"` | Background job tests pass |
| 9 | `dotnet test` | All tests pass |
| 10 | Manual review | Documentation complete |

---

## 9. COMMON GOTCHAS & PITFALLS

### 9.1 MassTransit Configuration

**GOTCHA:** Consumers not registered, messages go to _skipped queue
**SOLUTION:** Ensure all consumers registered with `AddConsumer<T>()` and `ConfigureEndpoints()` is called

**GOTCHA:** Queue names don't match expected pattern
**SOLUTION:** MassTransit auto-generates queue names from consumer type names, follow naming convention

**GOTCHA:** Messages published but not consumed
**SOLUTION:** Check RabbitMQ management UI, verify queues are bound to exchange

### 9.2 Background Services

**GOTCHA:** DbContext disposed error in background service
**SOLUTION:** Use `IServiceScopeFactory` to create scopes, get scoped services from scope

**GOTCHA:** Background service crashes and doesn't restart
**SOLUTION:** Wrap processing logic in try-catch to handle errors gracefully

**GOTCHA:** Background service runs once and stops
**SOLUTION:** Use infinite while loop with `!stoppingToken.IsCancellationRequested` check

### 9.3 Entity Framework

**GOTCHA:** SaveChangesAsync fails with concurrency issues
**SOLUTION:** Use proper transaction handling, consider optimistic concurrency for high-volume scenarios

**GOTCHA:** Enum stored as int instead of string
**SOLUTION:** Use `.HasConversion<string>()` in entity configuration (already documented in INITIAL.md)

### 9.4 Event Publishing

**GOTCHA:** Events published before SaveChangesAsync, order not in DB when consumer runs
**SOLUTION:** Always publish events AFTER SaveChangesAsync

**GOTCHA:** Consumer processes event but order not found
**SOLUTION:** Add defensive null checks in consumers, log warnings

### 9.5 Testing

**GOTCHA:** NSubstitute can't mock ConsumeContext
**SOLUTION:** Use `Substitute.For<ConsumeContext<TEvent>>()` and setup Message property

**GOTCHA:** Background job tests hang or timeout
**SOLUTION:** Use cancellation tokens properly, cancel after reasonable delay

**GOTCHA:** Randomness in tests (50% completion rate) causes flaky tests
**SOLUTION:** Either accept non-determinism or refactor consumer to inject Random for testing

### 9.6 RabbitMQ Docker

**GOTCHA:** RabbitMQ container not healthy, application fails to start
**SOLUTION:** Check container logs, ensure health check passes before starting app

**GOTCHA:** Can't access RabbitMQ management UI
**SOLUTION:** Verify port 15672 is mapped correctly, use guest/guest credentials

---

## 10. DOCUMENTATION UPDATES

### 10.1 CLAUDE.md Updates

Add these sections to `C:\Projects\OrderProcessingSystem\CLAUDE.md`:

```markdown
## Event-Driven Architecture

### Overview

The application uses MassTransit with RabbitMQ for event-driven communication:
- **Events**: Immutable records representing domain events (OrderCreated, OrderCompleted, OrderExpired)
- **Publishers**: Handlers and jobs that publish events using `IPublishEndpoint`
- **Consumers**: Classes implementing `IConsumer<TEvent>` that react to events
- **Transport**: RabbitMQ running in Docker for message queuing

### Event Flow

```
CreateOrderHandler → OrderCreatedEvent → OrderCreatedConsumer
                                       ↓
                              (50% success rate)
                                       ↓
                            OrderCompletedEvent → OrderCompletedNotificationConsumer
                                                ↓
                                          Save Notification
                                          Log Email (mocked)

OrderExpirationJob → OrderExpiredEvent → OrderExpiredNotificationConsumer
  (runs every 60s)                              ↓
                                          Save Notification
```

### Adding New Events

When adding a new event:

1. **Create Event Record** in `ModuleName/Events/`
   ```csharp
   public record SomethingHappenedEvent(Guid Id, DateTime OccurredAt);
   ```

2. **Publish Event** using `IPublishEndpoint`
   ```csharp
   await _publishEndpoint.Publish(new SomethingHappenedEvent(...), cancellationToken);
   ```

3. **Create Consumer** in `ModuleName/EventConsumers/`
   ```csharp
   public class SomethingHappenedConsumer : IConsumer<SomethingHappenedEvent>
   {
       public async Task Consume(ConsumeContext<SomethingHappenedEvent> context)
       {
           // Handle event
       }
   }
   ```

4. **Register Consumer** in `Program.cs`
   ```csharp
   x.AddConsumer<SomethingHappenedConsumer>();
   ```

5. **Write Tests** for publisher and consumer

### Background Jobs

Background jobs use `BackgroundService` from .NET:

- Run continuously in the background
- Use `IServiceScopeFactory` to create scopes for scoped dependencies
- Implement error handling to prevent crashes
- Use `ILogger` for observability

**Pattern:**
```csharp
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    while (!stoppingToken.IsCancellationRequested)
    {
        try
        {
            await DoWork(stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in background job");
        }

        await Task.Delay(interval, stoppingToken);
    }
}
```

### RabbitMQ Management

- **Management UI**: http://localhost:15672 (guest/guest)
- **View Queues**: See all queues, message counts, consumer connections
- **Debug Messages**: View message content, retry failed messages
- **Monitor Performance**: Message rates, memory usage

### Notifications Module

The Notifications module provides an audit trail for order events:

- **Notification Entity**: Stores notification records in database
- **NotificationType Enum**: OrderCompleted, OrderExpired
- **Consumers**: React to events and save notifications
- **Email Notifications**: Currently mocked (logged to console), can be replaced with real email service

### Common Commands

```bash
# Start RabbitMQ
docker-compose up -d rabbitmq

# View RabbitMQ logs
docker-compose logs -f rabbitmq

# Restart RabbitMQ
docker-compose restart rabbitmq

# Stop RabbitMQ
docker-compose stop rabbitmq

# Access RabbitMQ Management UI
# http://localhost:15672 (guest/guest)
```
```

---

## 11. CRITICAL FILES FOR IMPLEMENTATION

Based on this comprehensive PRP, the 10 most critical files for successful implementation are:

1. **docker-compose.yml** - Add RabbitMQ service (foundation for messaging)

2. **WebApi/Program.cs** - Register MassTransit, consumers, background jobs (central configuration)

3. **WebApi/Orders/Features/CreateOrder/CreateOrderHandler.cs** - Publish OrderCreatedEvent (trigger for entire flow)

4. **WebApi/Orders/EventConsumers/OrderCreatedConsumer.cs** - Process orders asynchronously (core business logic)

5. **WebApi/Notifications/Domain/Notification.cs** - Notification entity (audit trail foundation)

6. **WebApi/Notifications/EventConsumers/OrderCompletedNotificationConsumer.cs** - Handle completed orders (notification logic)

7. **WebApi/Jobs/OrderExpirationJob.cs** - Background job for expiration (automated processing)

8. **WebApi/Dal/OrderProcessingDbContext.cs** - Add Notifications DbSet (database foundation)

9. **WebApi/appsettings.json** - Add RabbitMQ configuration (application settings)

10. **Tests/WebApi.Tests/OrderCreatedConsumerTests.cs** - Consumer unit tests (validation pattern)

These files establish the event-driven patterns and infrastructure that all other features depend on.

---

## 12. DOCUMENTATION RESOURCES

### 12.1 Official Documentation

- **MassTransit Documentation:** https://masstransit.io/documentation/concepts
- **MassTransit RabbitMQ Transport:** https://masstransit.io/documentation/configuration/transports/rabbitmq
- **MassTransit Consumers:** https://masstransit.io/documentation/concepts/consumers
- **Background Services in .NET:** https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-9.0
- **RabbitMQ Official Docs:** https://www.rabbitmq.com/documentation.html
- **NSubstitute Documentation:** https://nsubstitute.github.io/

### 12.2 Tutorials & Examples (2025)

- **MassTransit with RabbitMQ Practical Guide (Feb 2025):** https://hamedsalameh.com/rabbitmq-and-masstransit-in-net-core-practical-guide/
- **Building Event-Driven .NET Application with MassTransit:** https://wrapt.dev/blog/building-an-event-driven-dotnet-application-setting-up-masstransit-and-rabbitmq
- **MassTransit with RabbitMQ in ASP.NET Core:** https://code-maze.com/masstransit-rabbitmq-aspnetcore/
- **Beginner's Guide to MassTransit and RabbitMQ:** https://www.rahulpnath.com/blog/masstransit-rabbitmq-asp-dotnet
- **Getting Started with RabbitMQ Pub/Sub using MassTransit:** https://dev.to/gaurav-nandankar/getting-started-with-rabbitmq-pubsub-using-masstransit-in-net-52m5
- **Efficient Background Jobs in .NET 8 (Apr 2025):** https://dev.to/leandroveiga/efficient-background-jobs-scheduled-tasks-in-net-8-with-hosted-services-50pk
- **Recurring Tasks in .NET C#:** https://dev.to/zrebhi/recurring-tasks-in-net-c-all-options-explained-jje
- **SE Radio: MassTransit and Event-Driven Systems (Feb 2025):** https://se-radio.net/2025/02/se-radio-654-chris-patterson-on-masstransit-and-event-driven-systems/

### 12.3 GitHub Examples

- **MassTransit GitHub Repository:** https://github.com/MassTransit/MassTransit
- **MassTransit Event-Driven Architecture Example:** https://github.com/jeanlimalopes/MassTransit-EventDrivenArchitecture

---

## 13. QUALITY SCORE & RISK ASSESSMENT

### 13.1 Confidence Score: 8.5/10

**Strengths:**
- Clear business requirements with specific rules (50% success rate, 10-minute expiration)
- Existing codebase provides strong foundation with established patterns
- Well-documented MassTransit integration (official docs + recent tutorials)
- Validation gates at each phase ensure incremental progress
- Comprehensive code examples for all new patterns
- External dependencies (RabbitMQ) containerized and easy to setup

**Risks (reducing score from 10 to 8.5):**

1. **Integration Risk (0.5 points):** RabbitMQ must be running before application starts
   - **Mitigation:** Health checks in docker-compose, clear error messages, startup order

2. **Timing/Concurrency Risk (0.5 points):** Background job + event consumers may process same order
   - **Mitigation:** Event consumers should be idempotent, defensive null checks, proper logging

3. **Testing Complexity (0.5 points):** Randomness in consumer (50% rate) may cause flaky tests
   - **Mitigation:** Accept non-determinism or refactor to inject Random, use proper assertions

### 13.2 Success Criteria

**Must Have (Critical):**
- [ ] RabbitMQ running in Docker on ports 5672 and 15672
- [ ] MassTransit registered and configured with RabbitMQ transport
- [ ] OrderCreatedEvent published when order created
- [ ] OrderCreatedConsumer processes orders asynchronously
- [ ] 50% of orders complete successfully, publish OrderCompletedEvent
- [ ] OrderExpirationJob runs every 60 seconds
- [ ] Orders in Processing >10 minutes expire to Expired status
- [ ] OrderExpiredEvent published for expired orders
- [ ] Notifications table created via migration
- [ ] Notification consumers save audit records to database
- [ ] Email notifications logged to console (mocked)
- [ ] All unit tests pass (minimum 4 new test classes)
- [ ] End-to-end flow works correctly

**Should Have (Important):**
- [ ] Comprehensive logging in all consumers and background jobs
- [ ] RabbitMQ management UI accessible for debugging
- [ ] Defensive null checks in all consumers
- [ ] Error handling in background job prevents crashes
- [ ] CLAUDE.md updated with event-driven patterns
- [ ] All consumers registered and queues created automatically

**Nice to Have (Optional):**
- [ ] Integration tests for end-to-end flow
- [ ] Retry policies for failed consumers
- [ ] Dead-letter queues for poison messages
- [ ] Monitoring/metrics for event processing

---

**END OF PRP**

**Prepared for:** Event-Driven Order Processing Implementation
**Target Framework:** .NET 10.0
**Date:** 2025-12-07
**Confidence Score:** 8.5/10
**External Dependencies:** RabbitMQ 3 (Docker), MassTransit 8.5.2
