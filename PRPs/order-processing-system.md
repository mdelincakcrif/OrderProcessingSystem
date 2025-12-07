# COMPREHENSIVE PRODUCT REQUIREMENTS & PLAN (PRP)
## Order Processing System REST API Implementation

---

## EXECUTIVE SUMMARY

This PRP provides a complete, one-pass implementation guide for building an Order Processing System REST API using ASP.NET Core 10.0 Minimal APIs, Entity Framework Core, PostgreSQL, MediatR, FluentValidation, and JWT authentication. The plan follows domain-driven design principles with CQRS pattern and includes comprehensive testing strategy.

**Confidence Score: 9/10** - High confidence for one-pass implementation success due to:
- Clear architectural patterns established
- Well-defined folder structure
- Specific code examples for all major patterns
- Validation gates at each phase
- Comprehensive documentation research completed

---

## 1. PROJECT STRUCTURE

### 1.1 Complete Folder Hierarchy

```
C:\Projects\OrderProcessingSystem\
├── docker-compose.yml                           # PostgreSQL container
├── README.md                                     # Updated with DB setup & run instructions
├── CLAUDE.md                                     # Updated with architecture findings
├── WebApi/
│   ├── WebApi.csproj                            # Updated with NuGet packages
│   ├── Program.cs                               # Application entry point
│   ├── appsettings.json                         # Production configuration
│   ├── appsettings.Development.json             # Development configuration
│   ├── Dal/                                     # Data Access Layer
│   │   ├── OrderProcessingDbContext.cs          # EF Core DbContext
│   │   ├── Migrations/                          # EF Core migrations folder
│   │   └── Configurations/                      # Entity configurations
│   │       ├── UserConfiguration.cs
│   │       ├── ProductConfiguration.cs
│   │       ├── OrderConfiguration.cs
│   │       └── OrderItemConfiguration.cs
│   ├── Common/                                  # Shared code
│   │   ├── Middleware/
│   │   │   └── ErrorHandlingMiddleware.cs       # Global error handler
│   │   ├── Extensions/
│   │   │   └── ValidationExtensions.cs          # Validation helpers
│   │   ├── Behaviors/
│   │   │   └── ValidationBehavior.cs            # MediatR validation pipeline
│   │   └── Security/
│   │       ├── JwtSettings.cs                   # JWT configuration model
│   │       └── JwtTokenService.cs               # JWT token generation
│   ├── Users/                                   # Users Module
│   │   ├── Domain/
│   │   │   └── User.cs                          # User entity
│   │   ├── Endpoints/
│   │   │   └── UsersEndpoints.cs                # Endpoint mappings
│   │   ├── Features/
│   │   │   ├── CreateUser/
│   │   │   │   ├── CreateUserCommand.cs         # MediatR request
│   │   │   │   ├── CreateUserHandler.cs         # MediatR handler
│   │   │   │   └── CreateUserValidator.cs       # FluentValidation
│   │   │   ├── GetUser/
│   │   │   │   ├── GetUserQuery.cs
│   │   │   │   └── GetUserHandler.cs
│   │   │   ├── GetAllUsers/
│   │   │   │   ├── GetAllUsersQuery.cs
│   │   │   │   └── GetAllUsersHandler.cs
│   │   │   ├── UpdateUser/
│   │   │   │   ├── UpdateUserCommand.cs
│   │   │   │   ├── UpdateUserHandler.cs
│   │   │   │   └── UpdateUserValidator.cs
│   │   │   └── DeleteUser/
│   │   │       ├── DeleteUserCommand.cs
│   │   │       └── DeleteUserHandler.cs
│   │   └── DTOs/
│   │       ├── CreateUserRequest.cs
│   │       ├── UpdateUserRequest.cs
│   │       └── UserResponse.cs
│   ├── Products/                                # Products Module
│   │   ├── Domain/
│   │   │   └── Product.cs                       # Product entity
│   │   ├── Endpoints/
│   │   │   └── ProductsEndpoints.cs
│   │   ├── Features/
│   │   │   ├── CreateProduct/
│   │   │   │   ├── CreateProductCommand.cs
│   │   │   │   ├── CreateProductHandler.cs
│   │   │   │   └── CreateProductValidator.cs
│   │   │   ├── GetProduct/
│   │   │   │   ├── GetProductQuery.cs
│   │   │   │   └── GetProductHandler.cs
│   │   │   ├── GetAllProducts/
│   │   │   │   ├── GetAllProductsQuery.cs
│   │   │   │   └── GetAllProductsHandler.cs
│   │   │   ├── UpdateProduct/
│   │   │   │   ├── UpdateProductCommand.cs
│   │   │   │   ├── UpdateProductHandler.cs
│   │   │   │   └── UpdateProductValidator.cs
│   │   │   └── DeleteProduct/
│   │   │       ├── DeleteProductCommand.cs
│   │   │       └── DeleteProductHandler.cs
│   │   └── DTOs/
│   │       ├── CreateProductRequest.cs
│   │       ├── UpdateProductRequest.cs
│   │       └── ProductResponse.cs
│   ├── Orders/                                  # Orders Module
│   │   ├── Domain/
│   │   │   ├── Order.cs                         # Order entity
│   │   │   ├── OrderItem.cs                     # Order item entity
│   │   │   └── OrderStatus.cs                   # OrderStatus enum
│   │   ├── Endpoints/
│   │   │   └── OrdersEndpoints.cs
│   │   ├── Features/
│   │   │   ├── CreateOrder/
│   │   │   │   ├── CreateOrderCommand.cs
│   │   │   │   ├── CreateOrderHandler.cs
│   │   │   │   └── CreateOrderValidator.cs
│   │   │   ├── GetOrder/
│   │   │   │   ├── GetOrderQuery.cs
│   │   │   │   └── GetOrderHandler.cs
│   │   │   ├── GetAllOrders/
│   │   │   │   ├── GetAllOrdersQuery.cs
│   │   │   │   └── GetAllOrdersHandler.cs
│   │   │   ├── UpdateOrder/
│   │   │   │   ├── UpdateOrderCommand.cs
│   │   │   │   ├── UpdateOrderHandler.cs
│   │   │   │   └── UpdateOrderValidator.cs
│   │   │   └── DeleteOrder/
│   │   │       ├── DeleteOrderCommand.cs
│   │   │       └── DeleteOrderHandler.cs
│   │   └── DTOs/
│   │       ├── CreateOrderRequest.cs
│   │       ├── UpdateOrderRequest.cs
│   │       ├── OrderResponse.cs
│   │       ├── CreateOrderItemRequest.cs
│   │       └── OrderItemResponse.cs
│   └── Authentication/                          # Authentication Module
│       ├── Endpoints/
│       │   └── AuthenticationEndpoints.cs
│       ├── Features/
│       │   └── Login/
│       │       ├── LoginCommand.cs
│       │       ├── LoginHandler.cs
│       │       └── LoginValidator.cs
│       └── DTOs/
│           ├── LoginRequest.cs
│           └── LoginResponse.cs
└── Tests/
    └── WebApi.Tests/
        ├── WebApi.Tests.csproj                  # Test project
        ├── TestFixtures/
        │   └── WebApiTestFixture.cs             # Shared test setup
        ├── Integration/
        │   ├── UsersEndpointsTests.cs           # Users module tests
        │   ├── ProductsEndpointsTests.cs        # Products module tests
        │   ├── OrdersEndpointsTests.cs          # Orders module tests
        │   ├── AuthenticationEndpointsTests.cs  # Auth module tests
        │   └── ErrorHandlingTests.cs            # Error handling tests
        └── Helpers/
            └── InMemoryDbHelper.cs              # In-memory DB setup
```

---

## 2. NUGET PACKAGES

### 2.1 WebApi Project Packages

Add these packages to `C:\Projects\OrderProcessingSystem\WebApi\WebApi.csproj`:

```xml
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="10.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.0">
  <PrivateAssets>all</PrivateAssets>
  <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
</PackageReference>
<PackageReference Include="MediatR" Version="12.4.1" />
<PackageReference Include="FluentValidation" Version="11.11.0" />
<PackageReference Include="FluentValidation.DependencyInjectionExtensions" Version="11.11.0" />
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="10.0.0" />
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="8.3.0" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="7.2.0" />
<PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
```

**Commands to add packages:**
```bash
dotnet add WebApi/WebApi.csproj package Npgsql.EntityFrameworkCore.PostgreSQL --version 10.0.0
dotnet add WebApi/WebApi.csproj package Microsoft.EntityFrameworkCore.Design --version 10.0.0
dotnet add WebApi/WebApi.csproj package MediatR --version 12.4.1
dotnet add WebApi/WebApi.csproj package FluentValidation --version 11.11.0
dotnet add WebApi/WebApi.csproj package FluentValidation.DependencyInjectionExtensions --version 11.11.0
dotnet add WebApi/WebApi.csproj package Microsoft.AspNetCore.Authentication.JwtBearer --version 10.0.0
dotnet add WebApi/WebApi.csproj package System.IdentityModel.Tokens.Jwt --version 8.3.0
dotnet add WebApi/WebApi.csproj package Swashbuckle.AspNetCore --version 7.2.0
dotnet add WebApi/WebApi.csproj package BCrypt.Net-Next --version 4.0.3
```

### 2.2 Test Project Packages

Add these packages to `C:\Projects\OrderProcessingSystem\Tests\WebApi.Tests\WebApi.Tests.csproj`:

```xml
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.12.0" />
<PackageReference Include="xunit" Version="2.9.3" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.8.2">
  <PrivateAssets>all</PrivateAssets>
  <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
</PackageReference>
<PackageReference Include="FluentAssertions" Version="7.0.0" />
<PackageReference Include="NSubstitute" Version="5.3.0" />
<PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="10.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="10.0.0" />
```

**Commands to add test packages:**
```bash
dotnet add Tests/WebApi.Tests/WebApi.Tests.csproj package Microsoft.NET.Test.Sdk --version 17.12.0
dotnet add Tests/WebApi.Tests/WebApi.Tests.csproj package xunit --version 2.9.3
dotnet add Tests/WebApi.Tests/WebApi.Tests.csproj package xunit.runner.visualstudio --version 2.8.2
dotnet add Tests/WebApi.Tests/WebApi.Tests.csproj package FluentAssertions --version 7.0.0
dotnet add Tests/WebApi.Tests/WebApi.Tests.csproj package NSubstitute --version 5.3.0
dotnet add Tests/WebApi.Tests/WebApi.Tests.csproj package Microsoft.AspNetCore.Mvc.Testing --version 10.0.0
dotnet add Tests/WebApi.Tests/WebApi.Tests.csproj package Microsoft.EntityFrameworkCore.InMemory --version 10.0.0
```

---

## 3. IMPLEMENTATION ORDER

### Phase 1: Foundation Setup (Validation: dotnet build succeeds)

1. **Update project files and configuration**
   - Add all NuGet packages to WebApi.csproj
   - Update appsettings.json with ConnectionStrings, JwtSettings
   - Update appsettings.Development.json with development settings
   - Create docker-compose.yml for PostgreSQL

2. **Create folder structure**
   - Create all module folders (Users/, Products/, Orders/, Authentication/, Common/, Dal/)
   - Create subfolders (Domain/, Features/, Endpoints/, DTOs/)

3. **Create domain entities**
   - `Users/Domain/User.cs`
   - `Products/Domain/Product.cs`
   - `Orders/Domain/Order.cs`
   - `Orders/Domain/OrderItem.cs`
   - `Orders/Domain/OrderStatus.cs`

4. **Setup Entity Framework**
   - Create `Dal/OrderProcessingDbContext.cs`
   - Create entity configurations in `Dal/Configurations/`
   - Add connection string and DbContext registration

**Validation Gate 1:** `dotnet build WebApi/WebApi.csproj` succeeds

### Phase 2: Infrastructure Layer (Validation: dotnet build succeeds)

5. **JWT Authentication Infrastructure**
   - Create `Common/Security/JwtSettings.cs`
   - Create `Common/Security/JwtTokenService.cs`
   - Configure JWT in Program.cs

6. **Error Handling Middleware**
   - Create `Common/Middleware/ErrorHandlingMiddleware.cs`
   - Register in Program.cs pipeline

7. **MediatR Setup**
   - Create `Common/Behaviors/ValidationBehavior.cs`
   - Register MediatR and behaviors in Program.cs

8. **Validation Extensions**
   - Create `Common/Extensions/ValidationExtensions.cs`

**Validation Gate 2:** `dotnet build WebApi/WebApi.csproj` succeeds

### Phase 3: Authentication Module (Validation: dotnet build succeeds)

9. **Authentication Feature**
   - Create `Authentication/DTOs/LoginRequest.cs`
   - Create `Authentication/DTOs/LoginResponse.cs`
   - Create `Authentication/Features/Login/LoginCommand.cs`
   - Create `Authentication/Features/Login/LoginValidator.cs`
   - Create `Authentication/Features/Login/LoginHandler.cs`
   - Create `Authentication/Endpoints/AuthenticationEndpoints.cs`

10. **Register Authentication Endpoints**
    - Update Program.cs to map authentication endpoints

**Validation Gate 3:** `dotnet build WebApi/WebApi.csproj` succeeds

### Phase 4: Users Module (Validation: dotnet build succeeds)

11. **Users DTOs**
    - Create `Users/DTOs/CreateUserRequest.cs`
    - Create `Users/DTOs/UpdateUserRequest.cs`
    - Create `Users/DTOs/UserResponse.cs`

12. **Users Features**
    - Create all CRUD features (CreateUser, GetUser, GetAllUsers, UpdateUser, DeleteUser)
    - Each feature includes: Command/Query, Handler, Validator (for Create/Update)

13. **Users Endpoints**
    - Create `Users/Endpoints/UsersEndpoints.cs`
    - Register in Program.cs

**Validation Gate 4:** `dotnet build WebApi/WebApi.csproj` succeeds

### Phase 5: Products Module (Validation: dotnet build succeeds)

14. **Products DTOs**
    - Create `Products/DTOs/CreateProductRequest.cs`
    - Create `Products/DTOs/UpdateProductRequest.cs`
    - Create `Products/DTOs/ProductResponse.cs`

15. **Products Features**
    - Create all CRUD features (CreateProduct, GetProduct, GetAllProducts, UpdateProduct, DeleteProduct)
    - Each feature includes: Command/Query, Handler, Validator (for Create/Update)

16. **Products Endpoints**
    - Create `Products/Endpoints/ProductsEndpoints.cs`
    - Register in Program.cs

**Validation Gate 5:** `dotnet build WebApi/WebApi.csproj` succeeds

### Phase 6: Orders Module (Validation: dotnet build succeeds)

17. **Orders DTOs**
    - Create `Orders/DTOs/CreateOrderRequest.cs`
    - Create `Orders/DTOs/UpdateOrderRequest.cs`
    - Create `Orders/DTOs/OrderResponse.cs`
    - Create `Orders/DTOs/CreateOrderItemRequest.cs`
    - Create `Orders/DTOs/OrderItemResponse.cs`

18. **Orders Features**
    - Create all CRUD features (CreateOrder, GetOrder, GetAllOrders, UpdateOrder, DeleteOrder)
    - Each feature includes: Command/Query, Handler, Validator (for Create/Update)

19. **Orders Endpoints**
    - Create `Orders/Endpoints/OrdersEndpoints.cs`
    - Register in Program.cs

**Validation Gate 6:** `dotnet build WebApi/WebApi.csproj` succeeds

### Phase 7: Database Setup (Validation: migration created, docker runs)

20. **EF Migrations**
    - Create initial migration: `dotnet ef migrations add InitialCreate --project WebApi`
    - Add seed data using ModelBuilder.HasData in DbContext

21. **Docker Compose**
    - Start PostgreSQL: `docker-compose up -d`
    - Apply migration: `dotnet ef database update --project WebApi`

**Validation Gate 7:**
- `dotnet ef migrations add InitialCreate --project WebApi/WebApi.csproj` succeeds
- `docker-compose up -d` starts PostgreSQL
- `dotnet ef database update --project WebApi/WebApi.csproj` succeeds

### Phase 8: Swagger Setup (Validation: dotnet run succeeds, Swagger UI accessible)

22. **Configure Swagger**
    - Add Swagger services in Program.cs
    - Configure JWT authentication in Swagger
    - Add XML documentation if needed

**Validation Gate 8:**
- `dotnet run --project WebApi/WebApi.csproj` succeeds
- Navigate to http://localhost:5153/swagger and UI loads

### Phase 9: Testing (Validation: dotnet test succeeds with >=5 passing tests)

23. **Create Test Project**
    - Create `Tests/WebApi.Tests/WebApi.Tests.csproj`
    - Add project reference to WebApi
    - Add all test NuGet packages

24. **Setup Test Infrastructure**
    - Create `Tests/WebApi.Tests/TestFixtures/WebApiTestFixture.cs`
    - Create `Tests/WebApi.Tests/Helpers/InMemoryDbHelper.cs`

25. **Write Integration Tests** (Minimum 5 test cases)
    - `Tests/WebApi.Tests/Integration/AuthenticationEndpointsTests.cs` (1-2 tests)
    - `Tests/WebApi.Tests/Integration/UsersEndpointsTests.cs` (1-2 tests)
    - `Tests/WebApi.Tests/Integration/ProductsEndpointsTests.cs` (1-2 tests)
    - `Tests/WebApi.Tests/Integration/OrdersEndpointsTests.cs` (1-2 tests)
    - `Tests/WebApi.Tests/Integration/ErrorHandlingTests.cs` (1-2 tests)

**Validation Gate 9:**
- `dotnet build Tests/WebApi.Tests/WebApi.Tests.csproj` succeeds
- `dotnet test Tests/WebApi.Tests/WebApi.Tests.csproj` succeeds with at least 5 passing tests

### Phase 10: Documentation & Cleanup (Validation: manual review)

26. **Update Documentation**
    - Update README.md with setup and run instructions
    - Update CLAUDE.md with architecture findings
    - Remove WeatherForecast endpoint from Program.cs

27. **Final Verification**
    - Run all tests: `dotnet test`
    - Build solution: `dotnet build`
    - Start application and test all endpoints via Swagger

**Validation Gate 10:**
- `dotnet build` succeeds for entire solution
- `dotnet test` succeeds with all tests passing
- Application runs and all endpoints accessible via Swagger
- README.md contains clear setup instructions

---

## 4. CODE PATTERNS

### 4.1 Domain Entity Pattern

**File:** `C:\Projects\OrderProcessingSystem\WebApi\Users\Domain\User.cs`

```csharp
namespace WebApi.Users.Domain;

public class User
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty; // BCrypt hashed
}
```

**File:** `C:\Projects\OrderProcessingSystem\WebApi\Products\Domain\Product.cs`

```csharp
namespace WebApi.Products.Domain;

public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

**File:** `C:\Projects\OrderProcessingSystem\WebApi\Orders\Domain\OrderStatus.cs`

```csharp
namespace WebApi.Orders.Domain;

public enum OrderStatus
{
    Pending,
    Processing,
    Completed,
    Expired
}
```

**File:** `C:\Projects\OrderProcessingSystem\WebApi\Orders\Domain\Order.cs`

```csharp
namespace WebApi.Orders.Domain;

public class Order
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public decimal Total { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<OrderItem> Items { get; set; } = new();
}
```

**File:** `C:\Projects\OrderProcessingSystem\WebApi\Orders\Domain\OrderItem.cs`

```csharp
namespace WebApi.Orders.Domain;

public class OrderItem
{
    public int Id { get; set; } // Primary key
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public Order Order { get; set; } = null!;
}
```

### 4.2 DTOs Pattern

**File:** `C:\Projects\OrderProcessingSystem\WebApi\Users\DTOs\CreateUserRequest.cs`

```csharp
namespace WebApi.Users.DTOs;

public record CreateUserRequest(
    string Name,
    string Email,
    string Password
);
```

**File:** `C:\Projects\OrderProcessingSystem\WebApi\Users\DTOs\UserResponse.cs`

```csharp
namespace WebApi.Users.DTOs;

public record UserResponse(
    Guid Id,
    string Name,
    string Email
);
```

### 4.3 MediatR Command/Query Pattern

**File:** `C:\Projects\OrderProcessingSystem\WebApi\Users\Features\CreateUser\CreateUserCommand.cs`

```csharp
using MediatR;
using WebApi.Users.DTOs;

namespace WebApi.Users.Features.CreateUser;

public record CreateUserCommand(
    string Name,
    string Email,
    string Password
) : IRequest<IResult>;
```

**File:** `C:\Projects\OrderProcessingSystem\WebApi\Products\Features\GetProduct\GetProductQuery.cs`

```csharp
using MediatR;

namespace WebApi.Products.Features.GetProduct;

public record GetProductQuery(Guid Id) : IRequest<IResult>;
```

### 4.4 MediatR Handler Pattern

**File:** `C:\Projects\OrderProcessingSystem\WebApi\Users\Features\CreateUser\CreateUserHandler.cs`

```csharp
using MediatR;
using Microsoft.EntityFrameworkCore;
using WebApi.Dal;
using WebApi.Users.Domain;
using WebApi.Users.DTOs;

namespace WebApi.Users.Features.CreateUser;

public class CreateUserHandler : IRequestHandler<CreateUserCommand, IResult>
{
    private readonly OrderProcessingDbContext _context;

    public CreateUserHandler(OrderProcessingDbContext context)
    {
        _context = context;
    }

    public async Task<IResult> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // Check if email already exists
        var emailExists = await _context.Users
            .AnyAsync(u => u.Email == request.Email, cancellationToken);

        if (emailExists)
        {
            return Results.BadRequest(new { error = "Email already exists" });
        }

        // Hash password
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Email = request.Email,
            Password = hashedPassword
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        var response = new UserResponse(user.Id, user.Name, user.Email);
        return Results.Created($"/api/users/{user.Id}", response);
    }
}
```

**File:** `C:\Projects\OrderProcessingSystem\WebApi\Products\Features\GetProduct\GetProductHandler.cs`

```csharp
using MediatR;
using Microsoft.EntityFrameworkCore;
using WebApi.Dal;
using WebApi.Products.DTOs;

namespace WebApi.Products.Features.GetProduct;

public class GetProductHandler : IRequestHandler<GetProductQuery, IResult>
{
    private readonly OrderProcessingDbContext _context;

    public GetProductHandler(OrderProcessingDbContext context)
    {
        _context = context;
    }

    public async Task<IResult> Handle(GetProductQuery request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (product == null)
        {
            return Results.NotFound(new { error = "Product not found" });
        }

        var response = new ProductResponse(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.Stock,
            product.CreatedAt
        );

        return Results.Ok(response);
    }
}
```

### 4.5 FluentValidation Pattern

**File:** `C:\Projects\OrderProcessingSystem\WebApi\Users\Features\CreateUser\CreateUserValidator.cs`

```csharp
using FluentValidation;

namespace WebApi.Users.Features.CreateUser;

public class CreateUserValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(100).WithMessage("Email must not exceed 100 characters");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters");
    }
}
```

**File:** `C:\Projects\OrderProcessingSystem\WebApi\Products\Features\CreateProduct\CreateProductValidator.cs`

```csharp
using FluentValidation;

namespace WebApi.Products.Features.CreateProduct;

public class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Price must be greater than or equal to 0");

        RuleFor(x => x.Stock)
            .GreaterThanOrEqualTo(0).WithMessage("Stock must be greater than or equal to 0");
    }
}
```

### 4.6 Endpoint Mapping Pattern

**File:** `C:\Projects\OrderProcessingSystem\WebApi\Users\Endpoints\UsersEndpoints.cs`

```csharp
using MediatR;
using Microsoft.AspNetCore.Authorization;
using WebApi.Users.Features.CreateUser;
using WebApi.Users.Features.GetUser;
using WebApi.Users.Features.GetAllUsers;
using WebApi.Users.Features.UpdateUser;
using WebApi.Users.Features.DeleteUser;
using WebApi.Users.DTOs;

namespace WebApi.Users.Endpoints;

public static class UsersEndpoints
{
    public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users")
            .WithTags("Users")
            .RequireAuthorization();

        group.MapPost("/", async (CreateUserRequest request, IMediator mediator) =>
        {
            var command = new CreateUserCommand(request.Name, request.Email, request.Password);
            return await mediator.Send(command);
        })
        .WithName("CreateUser")
        .WithOpenApi();

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var query = new GetUserQuery(id);
            return await mediator.Send(query);
        })
        .WithName("GetUser")
        .WithOpenApi();

        group.MapGet("/", async (IMediator mediator) =>
        {
            var query = new GetAllUsersQuery();
            return await mediator.Send(query);
        })
        .WithName("GetAllUsers")
        .WithOpenApi();

        group.MapPut("/{id:guid}", async (Guid id, UpdateUserRequest request, IMediator mediator) =>
        {
            var command = new UpdateUserCommand(id, request.Name, request.Email);
            return await mediator.Send(command);
        })
        .WithName("UpdateUser")
        .WithOpenApi();

        group.MapDelete("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var command = new DeleteUserCommand(id);
            return await mediator.Send(command);
        })
        .WithName("DeleteUser")
        .WithOpenApi();

        return app;
    }
}
```

**File:** `C:\Projects\OrderProcessingSystem\WebApi\Authentication\Endpoints\AuthenticationEndpoints.cs`

```csharp
using MediatR;
using WebApi.Authentication.Features.Login;
using WebApi.Authentication.DTOs;

namespace WebApi.Authentication.Endpoints;

public static class AuthenticationEndpoints
{
    public static IEndpointRouteBuilder MapAuthenticationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Authentication")
            .AllowAnonymous();

        group.MapPost("/login", async (LoginRequest request, IMediator mediator) =>
        {
            var command = new LoginCommand(request.Email, request.Password);
            return await mediator.Send(command);
        })
        .WithName("Login")
        .WithOpenApi();

        return app;
    }
}
```

### 4.7 Validation Behavior Pattern

**File:** `C:\Projects\OrderProcessingSystem\WebApi\Common\Behaviors\ValidationBehavior.cs`

```csharp
using FluentValidation;
using MediatR;

namespace WebApi.Common.Behaviors;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IResult
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken))
        );

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Any())
        {
            var errors = failures
                .GroupBy(f => f.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(f => f.ErrorMessage).ToArray()
                );

            return (TResponse)(IResult)Results.BadRequest(new { errors });
        }

        return await next();
    }
}
```

### 4.8 Error Handling Middleware Pattern

**File:** `C:\Projects\OrderProcessingSystem\WebApi\Common\Middleware\ErrorHandlingMiddleware.cs`

```csharp
using System.Net;
using System.Text.Json;

namespace WebApi.Common.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = HttpStatusCode.InternalServerError;
        var message = "An internal server error occurred";

        // You can add more specific exception handling here
        // For example: if (exception is NotFoundException) statusCode = HttpStatusCode.NotFound;

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            error = message,
            detail = exception.Message
        };

        var json = JsonSerializer.Serialize(response);
        return context.Response.WriteAsync(json);
    }
}
```

### 4.9 JWT Token Service Pattern

**File:** `C:\Projects\OrderProcessingSystem\WebApi\Common\Security\JwtSettings.cs`

```csharp
namespace WebApi.Common.Security;

public class JwtSettings
{
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; }
}
```

**File:** `C:\Projects\OrderProcessingSystem\WebApi\Common\Security\JwtTokenService.cs`

```csharp
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WebApi.Users.Domain;

namespace WebApi.Common.Security;

public class JwtTokenService
{
    private readonly JwtSettings _jwtSettings;

    public JwtTokenService(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
    }

    public string GenerateToken(User user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

### 4.10 Login Handler Pattern

**File:** `C:\Projects\OrderProcessingSystem\WebApi\Authentication\Features\Login\LoginHandler.cs`

```csharp
using MediatR;
using Microsoft.EntityFrameworkCore;
using WebApi.Common.Security;
using WebApi.Dal;
using WebApi.Authentication.DTOs;

namespace WebApi.Authentication.Features.Login;

public class LoginHandler : IRequestHandler<LoginCommand, IResult>
{
    private readonly OrderProcessingDbContext _context;
    private readonly JwtTokenService _jwtTokenService;

    public LoginHandler(OrderProcessingDbContext context, JwtTokenService jwtTokenService)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<IResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
        {
            return Results.Unauthorized();
        }

        var token = _jwtTokenService.GenerateToken(user);
        var response = new LoginResponse(token);

        return Results.Ok(response);
    }
}
```

### 4.11 DbContext Pattern

**File:** `C:\Projects\OrderProcessingSystem\WebApi\Dal\OrderProcessingDbContext.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using WebApi.Users.Domain;
using WebApi.Products.Domain;
using WebApi.Orders.Domain;

namespace WebApi.Dal;

public class OrderProcessingDbContext : DbContext
{
    public OrderProcessingDbContext(DbContextOptions<OrderProcessingDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderProcessingDbContext).Assembly);

        // Seed data
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Seed users
        var user1Id = Guid.NewGuid();
        var user2Id = Guid.NewGuid();

        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = user1Id,
                Name = "John Doe",
                Email = "john@example.com",
                Password = BCrypt.Net.BCrypt.HashPassword("password123")
            },
            new User
            {
                Id = user2Id,
                Name = "Jane Smith",
                Email = "jane@example.com",
                Password = BCrypt.Net.BCrypt.HashPassword("password123")
            }
        );

        // Seed products
        var product1Id = Guid.NewGuid();
        var product2Id = Guid.NewGuid();
        var product3Id = Guid.NewGuid();

        modelBuilder.Entity<Product>().HasData(
            new Product
            {
                Id = product1Id,
                Name = "Laptop",
                Description = "High-performance laptop",
                Price = 999.99m,
                Stock = 10,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Id = product2Id,
                Name = "Mouse",
                Description = "Wireless mouse",
                Price = 29.99m,
                Stock = 50,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Id = product3Id,
                Name = "Keyboard",
                Description = "Mechanical keyboard",
                Price = 79.99m,
                Stock = 30,
                CreatedAt = DateTime.UtcNow
            }
        );
    }
}
```

### 4.12 Entity Configuration Pattern

**File:** `C:\Projects\OrderProcessingSystem\WebApi\Dal\Configurations\UserConfiguration.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApi.Users.Domain;

namespace WebApi.Dal.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(u => u.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.Email)
            .HasColumnName("email")
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.Property(u => u.Password)
            .HasColumnName("password")
            .IsRequired();
    }
}
```

**File:** `C:\Projects\OrderProcessingSystem\WebApi\Dal\Configurations\OrderConfiguration.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApi.Orders.Domain;

namespace WebApi.Dal.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(o => o.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(o => o.Total)
            .HasColumnName("total")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(o => o.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(o => o.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(o => o.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasMany(o => o.Items)
            .WithOne(i => i.Order)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

**File:** `C:\Projects\OrderProcessingSystem\WebApi\Dal\Configurations\OrderItemConfiguration.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApi.Orders.Domain;

namespace WebApi.Dal.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("order_items");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(i => i.OrderId)
            .HasColumnName("order_id")
            .IsRequired();

        builder.Property(i => i.ProductId)
            .HasColumnName("product_id")
            .IsRequired();

        builder.Property(i => i.Quantity)
            .HasColumnName("quantity")
            .IsRequired();

        builder.Property(i => i.Price)
            .HasColumnName("price")
            .HasPrecision(18, 2)
            .IsRequired();
    }
}
```

---

## 5. CONFIGURATION FILES

### 5.1 appsettings.json

**File:** `C:\Projects\OrderProcessingSystem\WebApi\appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=54321;Database=orderprocessing;Username=postgres;Password=postgres"
  },
  "JwtSettings": {
    "Secret": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "OrderProcessingAPI",
    "Audience": "OrderProcessingClient",
    "ExpirationMinutes": 60
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### 5.2 appsettings.Development.json

**File:** `C:\Projects\OrderProcessingSystem\WebApi\appsettings.Development.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=54321;Database=orderprocessing;Username=postgres;Password=postgres"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Information",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  }
}
```

### 5.3 Program.cs

**File:** `C:\Projects\OrderProcessingSystem\WebApi\Program.cs`

```csharp
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using FluentValidation;
using WebApi.Common.Behaviors;
using WebApi.Common.Middleware;
using WebApi.Common.Security;
using WebApi.Dal;
using WebApi.Users.Endpoints;
using WebApi.Products.Endpoints;
using WebApi.Orders.Endpoints;
using WebApi.Authentication.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext
builder.Services.AddDbContext<OrderProcessingDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add MediatR
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

// Add FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

// Add JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.Configure<JwtSettings>(jwtSettings);
builder.Services.AddSingleton<JwtTokenService>();

var secret = jwtSettings.Get<JwtSettings>()?.Secret ?? throw new InvalidOperationException("JWT Secret not configured");
var key = Encoding.UTF8.GetBytes(secret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Get<JwtSettings>()?.Issuer,
        ValidAudience = jwtSettings.Get<JwtSettings>()?.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Order Processing API",
        Version = "v1",
        Description = "REST API for order processing system"
    });

    // Add JWT authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Configure middleware pipeline
app.UseMiddleware<ErrorHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

// Map endpoints
app.MapAuthenticationEndpoints();
app.MapUsersEndpoints();
app.MapProductsEndpoints();
app.MapOrdersEndpoints();

app.Run();

// Make Program class accessible to tests
public partial class Program { }
```

### 5.4 docker-compose.yml

**File:** `C:\Projects\OrderProcessingSystem\docker-compose.yml`

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
      - "54321:5432"
    volumes:
      - postgres-data:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres"]
      interval: 10s
      timeout: 5s
      retries: 5

volumes:
  postgres-data:
```

---

## 6. DATABASE SETUP

### 6.1 Migration Strategy

1. **Create Initial Migration:**
   ```bash
   dotnet ef migrations add InitialCreate --project WebApi/WebApi.csproj
   ```

2. **Apply Migration:**
   ```bash
   dotnet ef database update --project WebApi/WebApi.csproj
   ```

3. **Seed Data:**
   - Seed data is included in `OrderProcessingDbContext.OnModelCreating` using `HasData`
   - Seeds 2 users, 3 products (no orders initially)
   - All passwords are BCrypt hashed

### 6.2 Connection String

**Development:** `Host=localhost;Port=54321;Database=orderprocessing;Username=postgres;Password=postgres`

**Production:** Should be environment variable or Azure Key Vault

### 6.3 Docker Commands

```bash
# Start PostgreSQL
docker-compose up -d

# Stop PostgreSQL
docker-compose down

# View logs
docker-compose logs -f postgres

# Reset database (warning: deletes all data)
docker-compose down -v
docker-compose up -d
dotnet ef database update --project WebApi/WebApi.csproj
```

---

## 7. AUTHENTICATION & AUTHORIZATION

### 7.1 JWT Configuration

- **Secret:** Minimum 32 characters (stored in appsettings.json)
- **Issuer:** `OrderProcessingAPI`
- **Audience:** `OrderProcessingClient`
- **Expiration:** 60 minutes
- **Algorithm:** HMAC-SHA256

### 7.2 Password Hashing

- Use **BCrypt.Net-Next** for password hashing
- Hash passwords before storing in database
- Verify passwords using `BCrypt.Net.BCrypt.Verify()`

### 7.3 Protected Endpoints

All endpoints EXCEPT `/api/auth/login` require JWT Bearer token:

```http
Authorization: Bearer <token>
```

### 7.4 Swagger Authentication

Swagger UI includes "Authorize" button:
1. Click "Authorize"
2. Enter: `Bearer <your-token>`
3. Click "Authorize"
4. All subsequent requests include token

---

## 8. ERROR HANDLING

### 8.1 Error Response Format

All errors return consistent JSON format:

```json
{
  "error": "Error message",
  "detail": "Additional details (optional)"
}
```

### 8.2 HTTP Status Codes

- **400 Bad Request:** Validation errors, business rule violations
- **401 Unauthorized:** Missing or invalid JWT token
- **404 Not Found:** Resource not found
- **500 Internal Server Error:** Unhandled exceptions

### 8.3 Validation Errors Format

```json
{
  "errors": {
    "Name": ["Name is required", "Name must not exceed 100 characters"],
    "Email": ["Invalid email format"]
  }
}
```

### 8.4 Middleware Pipeline Order

1. ErrorHandlingMiddleware (catches all exceptions)
2. Swagger middleware
3. Authentication middleware
4. Authorization middleware
5. Endpoints

---

## 9. TESTING STRATEGY

### 9.1 Test Project Structure

**File:** `C:\Projects\OrderProcessingSystem\Tests\WebApi.Tests\WebApi.Tests.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.12.0" />
    <PackageReference Include="xunit" Version="2.9.3" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include="FluentAssertions" Version="7.0.0" />
    <PackageReference Include="NSubstitute" Version="5.3.0" />
    <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="10.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="10.0.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\WebApi\WebApi.csproj" />
  </ItemGroup>
</Project>
```

### 9.2 Test Fixture Pattern

**File:** `C:\Projects\OrderProcessingSystem\Tests\WebApi.Tests\TestFixtures\WebApiTestFixture.cs`

```csharp
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WebApi.Dal;

namespace WebApi.Tests.TestFixtures;

public class WebApiTestFixture : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the real DbContext
            services.RemoveAll(typeof(DbContextOptions<OrderProcessingDbContext>));

            // Add in-memory database
            services.AddDbContext<OrderProcessingDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDatabase");
            });

            // Ensure database is created
            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<OrderProcessingDbContext>();
            db.Database.EnsureCreated();
        });
    }
}
```

### 9.3 Test Example Pattern

**File:** `C:\Projects\OrderProcessingSystem\Tests\WebApi.Tests\Integration\AuthenticationEndpointsTests.cs`

```csharp
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using WebApi.Authentication.DTOs;
using WebApi.Tests.TestFixtures;
using Xunit;

namespace WebApi.Tests.Integration;

public class AuthenticationEndpointsTests : IClassFixture<WebApiTestFixture>
{
    private readonly HttpClient _client;

    public AuthenticationEndpointsTests(WebApiTestFixture fixture)
    {
        _client = fixture.CreateClient();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        // Arrange
        var request = new LoginRequest("john@example.com", "password123");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var request = new LoginRequest("john@example.com", "wrongpassword");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
```

### 9.4 Minimum Test Coverage (5+ Tests)

1. **AuthenticationEndpointsTests:**
   - Login with valid credentials returns token
   - Login with invalid credentials returns 401

2. **UsersEndpointsTests:**
   - Create user with valid data succeeds
   - Create user without authentication returns 401
   - Get user by ID returns correct user

3. **ProductsEndpointsTests:**
   - Create product with invalid data returns 400

4. **OrdersEndpointsTests:**
   - Create order with valid data succeeds

5. **ErrorHandlingTests:**
   - Unhandled exception returns 500

---

## 10. VALIDATION GATES

### Phase-by-Phase Validation Commands

**Phase 1-2: Foundation & Infrastructure**
```bash
dotnet build WebApi/WebApi.csproj
```
Expected: Build succeeds with no errors

**Phase 3-6: Modules Implementation**
```bash
dotnet build WebApi/WebApi.csproj
```
Expected: Build succeeds with no errors (after each module)

**Phase 7: Database Setup**
```bash
docker-compose up -d
dotnet ef migrations add InitialCreate --project WebApi/WebApi.csproj
dotnet ef database update --project WebApi/WebApi.csproj
```
Expected: Migration created, database updated successfully

**Phase 8: Application Running**
```bash
dotnet run --project WebApi/WebApi.csproj
```
Expected: Application starts, navigate to http://localhost:5153/swagger

**Phase 9: Testing**
```bash
dotnet test Tests/WebApi.Tests/WebApi.Tests.csproj
```
Expected: All tests pass (minimum 5 tests)

**Phase 10: Final Validation**
```bash
dotnet build
dotnet test
```
Expected: Entire solution builds, all tests pass

---

## 11. COMMON GOTCHAS & PITFALLS

### 11.1 EF Core Migrations

**GOTCHA:** Migrations fail if DbContext constructor is incorrect
**SOLUTION:** Ensure `OrderProcessingDbContext` has parameterless constructor OR proper DI registration

**GOTCHA:** Seed data with DateTime causes migration changes on every run
**SOLUTION:** Use specific UTC dates in seed data or generate GUIDs deterministically

### 11.2 MediatR with Minimal APIs

**GOTCHA:** Validation doesn't run automatically in Minimal APIs
**SOLUTION:** Use `ValidationBehavior<TRequest, TResponse>` in MediatR pipeline

**GOTCHA:** IResult return type causes issues with generic constraints
**SOLUTION:** Ensure handlers return `IResult` and ValidationBehavior uses `where TResponse : IResult`

### 11.3 FluentValidation

**GOTCHA:** Validators not automatically discovered in Minimal APIs
**SOLUTION:** Register with `AddValidatorsFromAssembly(typeof(Program).Assembly)`

**GOTCHA:** Validation errors return 500 instead of 400
**SOLUTION:** Implement `ValidationBehavior` to catch validation errors and return `Results.BadRequest()`

### 11.4 JWT Authentication

**GOTCHA:** JWT secret too short causes runtime error
**SOLUTION:** Ensure secret is at least 32 characters (256 bits for HMAC-SHA256)

**GOTCHA:** Token validation fails with clock skew
**SOLUTION:** Set `ClockSkew = TimeSpan.Zero` in `TokenValidationParameters`

**GOTCHA:** Swagger doesn't include Authorization header
**SOLUTION:** Configure `AddSecurityDefinition` and `AddSecurityRequirement` in Swagger

### 11.5 PostgreSQL with Docker

**GOTCHA:** Port 5432 already in use
**SOLUTION:** Use port 54321 as specified in requirements

**GOTCHA:** Connection string uses localhost but app in container
**SOLUTION:** Development uses localhost (host network), production might need service name

### 11.6 Entity Configuration

**GOTCHA:** Enum stored as int instead of string
**SOLUTION:** Use `.HasConversion<string>()` for OrderStatus enum

**GOTCHA:** Decimal precision causes data truncation
**SOLUTION:** Use `.HasPrecision(18, 2)` for Price and Total fields

### 11.7 Testing

**GOTCHA:** Tests fail due to shared database state
**SOLUTION:** Use unique in-memory database name per test class or reset between tests

**GOTCHA:** WebApplicationFactory<Program> doesn't find Program class
**SOLUTION:** Add `public partial class Program { }` at end of Program.cs

**GOTCHA:** Authentication required in tests
**SOLUTION:** Either mock JWT or use `.AllowAnonymous()` in test fixture override

### 11.8 Endpoint Routing

**GOTCHA:** Endpoints return 404 despite being registered
**SOLUTION:** Ensure `UseAuthentication()` and `UseAuthorization()` are called BEFORE endpoint mapping

**GOTCHA:** 401 instead of 404 for non-existent resources
**SOLUTION:** Order matters - authentication runs first, might return 401 before routing

### 11.9 Password Security

**GOTCHA:** Passwords stored in plain text
**SOLUTION:** Always hash with BCrypt before storing: `BCrypt.Net.BCrypt.HashPassword(password)`

**GOTCHA:** Password comparison fails
**SOLUTION:** Use `BCrypt.Net.BCrypt.Verify(plaintext, hashed)` not equality comparison

### 11.10 GUID Generation

**GOTCHA:** Empty GUIDs in database
**SOLUTION:** Use `Guid.NewGuid()` in handler, not in entity default value

**GOTCHA:** EF generates GUID instead of using provided one
**SOLUTION:** Use `.ValueGeneratedNever()` in entity configuration

---

## 12. DOCUMENTATION UPDATES

### 12.1 README.md Updates

**File:** `C:\Projects\OrderProcessingSystem\README.md`

Add sections for:

```markdown
## Prerequisites

- .NET 10.0 SDK
- Docker Desktop
- PostgreSQL client (optional, for manual database access)

## Database Setup

1. Start PostgreSQL container:
   ```bash
   docker-compose up -d
   ```

2. Apply migrations:
   ```bash
   dotnet ef database update --project WebApi/WebApi.csproj
   ```

3. Verify database:
   ```bash
   docker exec -it orderprocessing-postgres psql -U postgres -d orderprocessing -c "\dt"
   ```

## Running the Application

1. Start the API:
   ```bash
   dotnet run --project WebApi/WebApi.csproj
   ```

2. Open Swagger UI:
   ```
   http://localhost:5153/swagger
   ```

3. Test authentication:
   - Use Login endpoint with seed user: `john@example.com` / `password123`
   - Copy the returned token
   - Click "Authorize" button in Swagger
   - Enter: `Bearer <token>`
   - Now you can access protected endpoints

## Running Tests

```bash
dotnet test
```

## Seed Data

The application includes seed data:

**Users:**
- Email: john@example.com, Password: password123
- Email: jane@example.com, Password: password123

**Products:**
- Laptop ($999.99, Stock: 10)
- Mouse ($29.99, Stock: 50)
- Keyboard ($79.99, Stock: 30)
```

### 12.2 CLAUDE.md Updates

**File:** `C:\Projects\OrderProcessingSystem\CLAUDE.md`

Add sections for:

```markdown
## Architecture

### Domain-Driven Design
- Domain entities are EF Core entities (no separation)
- Rich domain models with behavior where appropriate
- Domain organized by modules (Users, Products, Orders, Authentication)

### CQRS with MediatR
- Commands for state changes (Create, Update, Delete)
- Queries for data retrieval (Get, GetAll)
- Each request has dedicated handler
- Validation via MediatR pipeline behavior

### Vertical Slice Architecture
- Organized by feature, not layer
- Each feature folder contains: Command/Query, Handler, Validator, DTOs
- Minimal cross-feature dependencies

### Module Organization
```
ModuleName/
├── Domain/          # Entities
├── Features/        # CQRS handlers (one folder per feature)
│   └── FeatureName/
│       ├── Command.cs
│       ├── Handler.cs
│       └── Validator.cs
├── Endpoints/       # Minimal API endpoint mappings
└── DTOs/            # Request/Response DTOs
```

## Key Patterns

### Validation
- FluentValidation for input validation
- Validation runs in MediatR pipeline before handler
- Returns 400 with structured error response

### Error Handling
- Global middleware catches all exceptions
- Returns consistent error format
- Logs exceptions for debugging

### Authentication
- JWT Bearer token authentication
- All endpoints protected except /api/auth/login
- Token includes user ID and email claims

## Database

### Migrations
```bash
# Create migration
dotnet ef migrations add MigrationName --project WebApi/WebApi.csproj

# Apply migrations
dotnet ef database update --project WebApi/WebApi.csproj

# Rollback
dotnet ef database update PreviousMigrationName --project WebApi/WebApi.csproj
```

### Seed Data
- Defined in OrderProcessingDbContext.OnModelCreating
- Uses HasData method
- Includes test users and products
```

---

## 13. QUALITY SCORE & RISK ASSESSMENT

### 13.1 Confidence Score: 9/10

**Strengths:**
- Clear, well-defined requirements with minimal ambiguity
- Proven technology stack (EF Core, MediatR, FluentValidation)
- Comprehensive code patterns provided for all major components
- Validation gates at each phase ensure incremental success
- Detailed folder structure eliminates organizational decisions
- Specific NuGet package versions avoid compatibility issues

**Risks (reducing score from 10 to 9):**
- **Minor Risk (0.5 points):** BCrypt password hashing in seed data might cause deterministic GUID issues in migrations
  - **Mitigation:** Use hardcoded GUIDs in seed data, generate hash once

- **Minor Risk (0.5 points):** In-memory database provider differences from PostgreSQL (e.g., unique constraints)
  - **Mitigation:** Test key scenarios, add note about PostgreSQL-specific behavior

### 13.2 Success Criteria

**Must Have (Critical):**
- [ ] All modules implement full CRUD operations
- [ ] JWT authentication protects all endpoints except login
- [ ] Validation returns 400 with error details
- [ ] PostgreSQL runs in Docker on port 54321
- [ ] Migrations create database schema
- [ ] Seed data populates initial records
- [ ] Swagger UI accessible and functional
- [ ] Minimum 5 integration tests pass
- [ ] Error handling middleware catches exceptions

**Should Have (Important):**
- [ ] FluentValidation validates all input DTOs
- [ ] MediatR handles all endpoint logic
- [ ] BCrypt hashes all passwords
- [ ] UTC timestamps on all date fields
- [ ] README documents setup and run instructions
- [ ] CLAUDE.md updated with architecture notes

**Nice to Have (Optional):**
- [ ] Additional test coverage beyond minimum 5
- [ ] XML documentation comments
- [ ] Health check endpoint
- [ ] Request/response logging

---

## 14. CRITICAL FILES FOR IMPLEMENTATION

Based on this comprehensive PRP, the 5 most critical files for successful implementation are:

1. **C:\Projects\OrderProcessingSystem\WebApi\Program.cs** - Central configuration hub: registers all services (DbContext, MediatR, FluentValidation, JWT), configures middleware pipeline, maps all endpoints

2. **C:\Projects\OrderProcessingSystem\WebApi\Dal\OrderProcessingDbContext.cs** - Database foundation: defines all DbSets, entity configurations, seed data; critical for EF migrations

3. **C:\Projects\OrderProcessingSystem\WebApi\Common\Behaviors\ValidationBehavior.cs** - Validation infrastructure: ensures FluentValidation runs for all requests, returns proper 400 errors, integrates with MediatR pipeline

4. **C:\Projects\OrderProcessingSystem\WebApi\Common\Middleware\ErrorHandlingMiddleware.cs** - Error handling foundation: catches unhandled exceptions, returns consistent error responses, implements 500 error requirement

5. **C:\Projects\OrderProcessingSystem\WebApi\Users\Features\CreateUser\CreateUserHandler.cs** - Pattern reference: exemplifies the handler pattern that all other CRUD operations will follow (validation, BCrypt hashing, database operations, response mapping)

These files establish the foundational patterns that all other implementation will replicate. Once these are correct, the remaining features (Products, Orders modules) follow identical patterns with different entity types.

---

## 15. DOCUMENTATION RESOURCES

### 15.1 Official Documentation

- **ASP.NET Core Minimal APIs:** https://learn.microsoft.com/en-us/aspnet/core/tutorials/min-web-api?view=aspnetcore-10.0
- **Entity Framework Core Migrations & Seeding:** https://learn.microsoft.com/en-us/ef/core/modeling/data-seeding
- **MediatR Official Wiki:** https://github.com/LuckyPennySoftware/MediatR/wiki
- **FluentValidation ASP.NET Core:** https://docs.fluentvalidation.net/en/latest/aspnet.html
- **JWT Authentication ASP.NET Core:** https://learn.microsoft.com/en-us/aspnet/core/security/authentication/configure-jwt-bearer-authentication?view=aspnetcore-10.0
- **Swagger/OpenAPI for Minimal APIs:** https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/aspnetcore-openapi?view=aspnetcore-9.0
- **Npgsql EF Core Provider:** https://www.npgsql.org/efcore/

### 15.2 Tutorials & Examples

- **Organizing Minimal APIs:** https://www.tessferrandez.com/blog/2023/10/31/organizing-minimal-apis.html
- **CQRS and MediatR:** https://codewithmukesh.com/blog/cqrs-and-mediatr-in-aspnet-core/
- **JWT with Minimal APIs:** https://www.ottorinobruni.com/how-to-implement-jwt-authentication-in-asp-net-core-minimal-api/
- **Docker Compose with PostgreSQL:** https://medium.com/codex/containerizing-a-net-app-with-postgres-using-docker-compose-a35167b419e7
- **DDD with EF Core:** https://thehonestcoder.com/ddd-ef-core-8/
- **xUnit, FluentAssertions, NSubstitute:** https://devofthings.hashnode.dev/writing-unit-tests-with-xunit-nsubstitute-and-fluentassertions

---

**END OF PRP**

**Prepared for:** Order Processing System Implementation
**Target Framework:** .NET 10.0
**Date:** 2025-12-06
**Confidence Score:** 9/10
