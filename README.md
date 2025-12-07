# Order Processing System - REST API

A comprehensive Order Processing System built with ASP.NET Core 10.0, featuring JWT authentication, PostgreSQL database, and complete CRUD operations for Users, Products, and Orders.

## Features

- **Authentication**: JWT Bearer token authentication
- **Users Module**: Complete CRUD operations with BCrypt password hashing
- **Products Module**: Product inventory management
- **Orders Module**: Order processing with line items
- **Validation**: FluentValidation for request validation
- **Architecture**: CQRS pattern using MediatR
- **Database**: PostgreSQL with Entity Framework Core
- **API Documentation**: Swagger/OpenAPI with JWT support
- **Testing**: Integration test framework using xUnit

## Technology Stack

- ASP.NET Core 10.0 (Minimal APIs)
- Entity Framework Core 10.0
- PostgreSQL 17 (via Docker)
- MediatR 12.4.1 (CQRS)
- FluentValidation 11.11.0
- JWT Bearer Authentication
- BCrypt.Net for password hashing
- Swaggeruck/OpenAPI
- xUnit + FluentAssertions (Testing)

## Prerequisites

- .NET 10.0 SDK
- Docker Desktop
- Git

## Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/mdelincakcrif/OrderProcessingSystem.git
cd OrderProcessingSystem
```

### 2. Start PostgreSQL Database

The application uses PostgreSQL running in Docker on port **5433**:

```bash
docker-compose up -d
```

Verify the container is running:

```bash
docker ps
```

### 3. Apply Database Migrations

Create and apply the database schema with seed data:

```bash
dotnet ef database update --project WebApi/WebApi.csproj
```

This will:
- Create the database schema
- Seed 2 users (john@example.com, jane@example.com)
- Seed 3 products (Laptop, Mouse, Keyboard)

### 4. Run the Application

```bash
dotnet run --project WebApi/WebApi.csproj
```

The API will be available at: **http://localhost:5153**

### 5. Access Swagger UI

Open your browser and navigate to:

```
http://localhost:5153/swagger
```

## API Endpoints

### Authentication (Anonymous)

- `POST /api/auth/login` - Login and receive JWT token

### Users (Requires Authentication)

- `POST /api/users` - Create a new user
- `GET /api/users` - Get all users
- `GET /api/users/{id}` - Get user by ID
- `PUT /api/users/{id}` - Update user
- `DELETE /api/users/{id}` - Delete user

### Products (Requires Authentication)

- `POST /api/products` - Create a new product
- `GET /api/products` - Get all products
- `GET /api/products/{id}` - Get product by ID
- `PUT /api/products/{id}` - Update product
- `DELETE /api/products/{id}` - Delete product

### Orders (Requires Authentication)

- `POST /api/orders` - Create a new order
- `GET /api/orders` - Get all orders
- `GET /api/orders/{id}` - Get order by ID
- `PUT /api/orders/{id}` - Update order
- `DELETE /api/orders/{id}` - Delete order

## Using the API

### 1. Login

```bash
curl -X POST http://localhost:5153/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"john@example.com","password":"password123"}'
```

Response:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

### 2. Use the Token

Include the token in the Authorization header for all authenticated endpoints:

```bash
curl -X GET http://localhost:5153/api/products \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

### 3. Swagger UI

Alternatively, use the Swagger UI:

1. Navigate to http://localhost:5153/swagger
2. Click "Authorize" button
3. Enter: `Bearer YOUR_TOKEN_HERE`
4. Click "Authorize"
5. Test endpoints directly in the browser

## Configuration

### Database Connection

Edit `WebApi/appsettings.json` or `WebApi/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5433;Database=orderprocessing;Username=postgres;Password=postgres"
  }
}
```

### JWT Settings

Configure in `WebApi/appsettings.json`:

```json
{
  "JwtSettings": {
    "Secret": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "OrderProcessingAPI",
    "Audience": "OrderProcessingClient",
    "ExpirationMinutes": 60
  }
}
```

**Important**: Use a strong, unique secret in production!

## Development

### Build the Solution

```bash
dotnet build
```

### Run with Hot Reload

```bash
dotnet watch --project WebApi/WebApi.csproj
```

### Run Tests

```bash
dotnet test Tests/WebApi.Tests/WebApi.Tests.csproj
```

Note: Integration tests use an in-memory database.

### Create a New Migration

After modifying domain entities:

```bash
dotnet ef migrations add MigrationName --project WebApi/WebApi.csproj
dotnet ef database update --project WebApi/WebApi.csproj
```

## Project Structure

```
OrderProcessingSystem/
├── WebApi/
│   ├── Authentication/       # Login endpoints and JWT
│   │   ├── DTOs/
│   │   ├── Features/
│   │   └── Endpoints/
│   ├── Users/               # Users CRUD module
│   │   ├── Domain/
│   │   ├── DTOs/
│   │   ├── Features/
│   │   └── Endpoints/
│   ├── Products/            # Products CRUD module
│   │   ├── Domain/
│   │   ├── DTOs/
│   │   ├── Features/
│   │   └── Endpoints/
│   ├── Orders/              # Orders CRUD module
│   │   ├── Domain/
│   │   ├── DTOs/
│   │   ├── Features/
│   │   └── Endpoints/
│   ├── Common/              # Shared infrastructure
│   │   ├── Behaviors/       # MediatR pipeline behaviors
│   │   ├── Middleware/      # Error handling middleware
│   │   └── Security/        # JWT services
│   ├── Dal/                 # Data Access Layer
│   │   ├── Configurations/  # EF Core configurations
│   │   └── Migrations/      # EF Core migrations
│   └── Program.cs           # Application entry point
├── Tests/
│   └── WebApi.Tests/        # Integration tests
├── docker-compose.yml       # PostgreSQL container
└── README.md
```

## Architecture

### Vertical Slice Architecture

Each feature is organized as a vertical slice containing all layers:
- DTOs (Data Transfer Objects)
- Commands/Queries (MediatR requests)
- Validators (FluentValidation)
- Handlers (Business logic)
- Endpoints (HTTP API)

### CQRS with MediatR

- Commands: Create, Update, Delete operations
- Queries: Read operations
- Handlers: Process requests and return results
- ValidationBehavior: Automatic request validation in pipeline

### Security

- **Authentication**: JWT Bearer tokens
- **Password Storage**: BCrypt hashing with salt
- **Authorization**: All endpoints except login require valid JWT
- **Validation**: FluentValidation prevents invalid data

## Troubleshooting

### Port 5433 Already in Use

If port 5433 is occupied, edit `docker-compose.yml`:

```yaml
ports:
  - "5432:5432"  # Change to different port
```

Then update the connection string in `appsettings.json`.

### Database Connection Failed

Ensure PostgreSQL container is running:

```bash
docker ps
docker logs orderprocessing-postgres
```

### EF Core Tools Not Found

Install globally:

```bash
dotnet tool install --global dotnet-ef
```

## Seed Data

The database is seeded with test data:

**Users:**
- john@example.com / password123
- jane@example.com / password123

**Products:**
- Laptop ($999.99, stock: 10)
- Mouse ($29.99, stock: 50)
- Keyboard ($79.99, stock: 30)

## License

This project is for educational and demonstration purposes.

## Contributing

This is a demonstration project. For issues or suggestions, please open an issue on GitHub.
