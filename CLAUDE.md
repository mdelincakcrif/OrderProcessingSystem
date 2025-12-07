# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Important: Prompt Logging

**CRITICAL INSTRUCTION:** Every prompt you receive and execute must be logged to the GitHub wiki.

**Process:**
1. After completing any prompt, log it to `C:\Projects\OrderProcessingSystem\OrderProcessingSystem.wiki\Prompts.md`
2. Include:
   - Prompt number (sequential)
   - Duration estimate
   - Timestamp (if significant work like migrations)
   - Exact prompt text
   - Description of what was done
   - Results/outcomes
3. Commit and push changes to the wiki repository
4. Use format matching existing entries in Prompts.md

**Wiki Repository:** The OrderProcessingSystem.wiki directory is a separate git repository for the GitHub wiki at https://github.com/mdelincakcrif/OrderProcessingSystem.wiki

## Project Overview

This is a complete Order Processing System REST API built with ASP.NET Core 10.0, featuring JWT authentication, PostgreSQL database, and comprehensive CRUD operations for Users, Products, and Orders.

## Architecture

### Vertical Slice Architecture

The project uses a Vertical Slice Architecture where each feature is self-contained:

- **Authentication/** - Login and JWT token generation
- **Users/** - User management (CRUD)
- **Products/** - Product catalog management (CRUD)
- **Orders/** - Order processing with line items (CRUD)
- **Common/** - Shared infrastructure (middleware, behaviors, security)
- **Dal/** - Data Access Layer (EF Core context, configurations, migrations)

Each module follows this structure:
```
ModuleName/
├── Domain/          # Entity classes
├── DTOs/            # Request/Response objects
├── Features/        # CQRS features
│   ├── Create*/     # Create operations
│   ├── Get*/        # Query operations
│   ├── Update*/     # Update operations
│   └── Delete*/     # Delete operations
└── Endpoints/       # HTTP endpoint mappings
```

### CQRS with MediatR

The application implements Command Query Responsibility Segregation (CQRS) using MediatR:

- **Commands**: Create, Update, Delete operations that modify state
- **Queries**: Get, GetAll operations that only read state
- **Handlers**: Process requests (`IRequestHandler<TRequest, IResult>`)
- **Validators**: FluentValidation for input validation
- **Behaviors**: Pipeline behaviors for cross-cutting concerns (validation)

### Key Patterns

1. **DTOs as Records**: Immutable request/response objects
   ```csharp
   public record CreateUserRequest(string Name, string Email, string Password);
   ```

2. **Commands/Queries as Records**: Implementing `IRequest<IResult>`
   ```csharp
   public record CreateUserCommand(string Name, string Email, string Password) : IRequest<IResult>;
   ```

3. **Handlers**: Implement business logic
   ```csharp
   public class CreateUserHandler : IRequestHandler<CreateUserCommand, IResult>
   {
       public async Task<IResult> Handle(CreateUserCommand request, CancellationToken cancellationToken)
       {
           // Business logic here
           return Results.Created($"/api/users/{id}", response);
       }
   }
   ```

4. **Validators**: Inherit from `AbstractValidator<T>`
   ```csharp
   public class CreateUserValidator : AbstractValidator<CreateUserCommand>
   {
       public CreateUserValidator()
       {
           RuleFor(x => x.Email).NotEmpty().EmailAddress();
       }
   }
   ```

5. **Endpoints**: Static extension methods
   ```csharp
   public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder app)
   {
       var group = app.MapGroup("/api/users").WithTags("Users").RequireAuthorization();
       // Map endpoints...
   }
   ```

## Project Structure

- **WebApi/** - Main Web API project
  - `Program.cs` - Application entry point, service registration, middleware pipeline
  - `WebApi.csproj` - Project file targeting net10.0
  - `appsettings.json` / `appsettings.Development.json` - Configuration files
  - `Properties/launchSettings.json` - Launch profiles for development
  - **Authentication/** - Login and JWT authentication
    - **DTOs/** - LoginRequest, LoginResponse
    - **Features/Login/** - LoginCommand, LoginHandler, LoginValidator
    - **Endpoints/** - AuthenticationEndpoints (AllowAnonymous)
  - **Users/** - Users module (CRUD)
    - **Domain/** - User entity
    - **DTOs/** - CreateUserRequest, UpdateUserRequest, UserResponse
    - **Features/** - CreateUser, GetUser, GetAllUsers, UpdateUser, DeleteUser
    - **Endpoints/** - UsersEndpoints (RequireAuthorization)
  - **Products/** - Products module (CRUD)
    - **Domain/** - Product entity
    - **DTOs/** - CreateProductRequest, UpdateProductRequest, ProductResponse
    - **Features/** - CreateProduct, GetProduct, GetAllProducts, UpdateProduct, DeleteProduct
    - **Endpoints/** - ProductsEndpoints (RequireAuthorization)
  - **Orders/** - Orders module (CRUD with line items)
    - **Domain/** - Order entity, OrderItem entity, OrderStatus enum
    - **DTOs/** - CreateOrderRequest, UpdateOrderRequest, OrderResponse, OrderItemResponse
    - **Features/** - CreateOrder, GetOrder, GetAllOrders, UpdateOrder, DeleteOrder
    - **Endpoints/** - OrdersEndpoints (RequireAuthorization)
  - **Common/** - Shared infrastructure
    - **Behaviors/** - ValidationBehavior (MediatR pipeline)
    - **Middleware/** - ErrorHandlingMiddleware (global exception handling)
    - **Security/** - JwtSettings, JwtTokenService
  - **Dal/** - Data Access Layer
    - `OrderProcessingDbContext.cs` - EF Core context with seed data
    - **Configurations/** - Entity configurations
      - `UserConfiguration.cs` - User entity configuration
      - `ProductConfiguration.cs` - Product entity configuration
      - `OrderConfiguration.cs` - Order entity configuration
      - `OrderItemConfiguration.cs` - OrderItem entity configuration
    - **Migrations/** - EF Core migrations
- **Tests/** - Test projects
  - **WebApi.Tests/** - Integration tests using WebApplicationFactory
    - `WebApiTestFixture.cs` - Test fixture with in-memory database
    - `AuthenticationEndpointsTests.cs` - Login endpoint tests
    - `UsersEndpointsTests.cs` - Users CRUD tests
    - `ProductsEndpointsTests.cs` - Products CRUD tests
    - `OrdersEndpointsTests.cs` - Orders CRUD tests
    - `ErrorHandlingTests.cs` - Validation and error handling tests

## Common Commands

### Build and Run

```bash
# Build the solution
dotnet build WebApi/WebApi.csproj

# Run the Web API (runs on http://localhost:5153)
dotnet run --project WebApi/WebApi.csproj

# Run in watch mode for development
dotnet watch --project WebApi/WebApi.csproj
```

### Database Commands

```bash
# Start PostgreSQL container
docker-compose up -d

# Create a new migration
dotnet ef migrations add MigrationName --project WebApi/WebApi.csproj

# Apply migrations to database
dotnet ef database update --project WebApi/WebApi.csproj

# Remove last migration
dotnet ef migrations remove --project WebApi/WebApi.csproj

# Drop database and recreate
dotnet ef database drop --project WebApi/WebApi.csproj --force
dotnet ef database update --project WebApi/WebApi.csproj
```

### Testing

```bash
# Run all tests
dotnet test

# Run tests for a specific project
dotnet test Tests/WebApi.Tests/WebApi.Tests.csproj

# Run a single test
dotnet test --filter "FullyQualifiedName=WebApi.Tests.AuthenticationEndpointsTests.Login_WithValidCredentials_ReturnsOkAndToken"
```

### Other Useful Commands

```bash
# Restore dependencies
dotnet restore

# Clean build artifacts
dotnet clean

# Add a NuGet package
dotnet add WebApi/WebApi.csproj package <PackageName>
```

## Architecture Notes

### Technology Stack

- **Framework**: ASP.NET Core 10.0 (Minimal APIs)
- **Database**: PostgreSQL 17 (port 5433)
- **ORM**: Entity Framework Core 10.0
- **CQRS**: MediatR 12.4.1
- **Validation**: FluentValidation 11.11.0
- **Authentication**: JWT Bearer (Microsoft.AspNetCore.Authentication.JwtBearer)
- **Password Hashing**: BCrypt.Net-Next 4.0.3
- **API Documentation**: Swashbuckle.AspNetCore 7.2.0
- **Testing**: xUnit 2.9.2, FluentAssertions 6.12.2, Microsoft.AspNetCore.Mvc.Testing

### Service Registration (Program.cs)

The application configures services in this order:

1. **DbContext** - PostgreSQL with Npgsql provider
2. **MediatR** - With ValidationBehavior pipeline
3. **FluentValidation** - Auto-register validators from assembly
4. **JWT Authentication** - Bearer token authentication
5. **Authorization** - Role-based authorization
6. **Swagger** - OpenAPI with JWT security scheme

### Middleware Pipeline (Program.cs)

The request pipeline is configured in this order:

1. **ErrorHandlingMiddleware** - Global exception handling (500 errors)
2. **Swagger** - Development only
3. **Authentication** - JWT token validation
4. **Authorization** - Enforce authorization policies
5. **Endpoint Routing** - Map module endpoints

### Entity Framework Configurations

All entities use explicit configurations in `Dal/Configurations/`:

- **Primary Keys**: GUIDs use `ValueGeneratedNever()`
- **Decimal Fields**: Use `.HasPrecision(18, 2)` for currency
- **Enums**: Use `.HasConversion<string>()` for string storage
- **Indexes**: Unique indexes on Email fields
- **Relationships**: Configured with cascade delete where appropriate

### Security

- **Authentication**: JWT Bearer tokens (60-minute expiration)
- **Password Storage**: BCrypt hashing with salt (never store plaintext)
- **Authorization**: All endpoints require authentication except `/api/auth/login`
- **Validation**: FluentValidation prevents invalid data from reaching handlers
- **Error Handling**: ErrorHandlingMiddleware catches exceptions and returns 500

### Seed Data

The database is seeded with test data in `OrderProcessingDbContext.SeedData()`:

**Users:**
- john@example.com / password123 (ID: 11111111-1111-1111-1111-111111111111)
- jane@example.com / password123 (ID: 22222222-2222-2222-2222-222222222222)

**Products:**
- Laptop - $999.99, Stock: 10 (ID: 33333333-3333-3333-3333-333333333333)
- Mouse - $29.99, Stock: 50 (ID: 44444444-4444-4444-4444-444444444444)
- Keyboard - $79.99, Stock: 30 (ID: 55555555-5555-5555-5555-555555555555)

**Important**: Seed data uses fixed GUIDs and UTC dates to prevent migration changes.

### Adding New Features

When adding a new feature, follow this pattern:

1. **Create Domain Entity** in `ModuleName/Domain/`
2. **Create DTOs** in `ModuleName/DTOs/`
3. **Create Commands/Queries** in `ModuleName/Features/FeatureName/`
4. **Create Validators** in `ModuleName/Features/FeatureName/`
5. **Create Handlers** in `ModuleName/Features/FeatureName/`
6. **Create Endpoints** in `ModuleName/Endpoints/`
7. **Register Endpoints** in `Program.cs`
8. **Create Entity Configuration** in `Dal/Configurations/`
9. **Create Migration** using `dotnet ef migrations add`
10. **Create Tests** in `Tests/WebApi.Tests/`

### Common Gotchas

- **BCrypt Hashes**: Cannot use `BCrypt.HashPassword()` in seed data (non-deterministic). Use pre-computed hashes.
- **GUIDs**: Must use `ValueGeneratedNever()` for seeded entities
- **UTC Dates**: Always use `DateTime.UtcNow` and `DateTimeKind.Utc` in seed data
- **Decimals**: Currency fields need `.HasPrecision(18, 2)`
- **Enums**: Must use `.HasConversion<string>()` for readable values in database
- **Migrations**: Changes to seed data trigger new migrations
- **JWT Secret**: Must be ≥32 characters
- **Test Database**: WebApiTestFixture uses in-memory database, not PostgreSQL

### Testing Approach

Integration tests use `WebApplicationFactory<Program>` with:
- In-memory database (Microsoft.EntityFrameworkCore.InMemory)
- Replaces PostgreSQL DbContext registration
- Unique database name per test run to avoid conflicts
- No need for Docker/PostgreSQL during tests

Test pattern:
1. Arrange: Create request objects
2. Act: Send HTTP request via test client
3. Assert: Verify response status and content using FluentAssertions
