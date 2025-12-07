## FEATURE:
We are going to implement REST API service for products ordering system.

Users module:
 - User has following fields: id, name max length 100, email max length 100 and is unique, password string
 - Create CRUD REST API for this module
 - Validate input DTOs. If wrong return 400

Authetication module:
 - Login REST API
 - Check user credentials (email, password) and if correct return JWT token

Products module:
 - Product has following fields: id, name string max length 100, description string, price number >= 0, stock number >= 0, created_at timestamp
 - Create CRUD REST API for this module
 - Validate input DTOs. If wrong return 400

Orders module:
 - Order has following fields: id, user_id , total number >= 0, status enum (pending, processing, completed, expired), items schema id primary key, product_id, quantity number > 0, price number > 0 created_at timestamp, updated_at timestamp
 - Create CRUD REST API for this module
 - Validate input DTOs. The rules are in scheme

Additional requirements
 - Endpoints has to be protected with JWT Bearer token. Result of Login REST API.
 - Correctly handle error return states    
 	- 400 Bad Request
    - 401 Unauthorized
    - 404 Not Found
    - 500 Internal Server Error
 - Include OpenAPI/Swagger documentation
 - Integration tests (minimum 5 test cases)
 - Use Postgres DBS. Run Postgres in docker and initialize it with docker compose file. Include docker compose file in the GIT repository. Use port 54321 for Postgres becuase 5432 can be busy on developer machines.
 - Include into the final solution DB upgrade mechanism. It has to contain some form of upgrade DB scripts or DB upgrade code.
 - Include into DBS also initial seed data. Can be part of the upgrade menchanism too.
 - In Readme.md document how to run DB upgrade tool and how to start the service.
 - all ids are GUIDs
 - every date is in UTC

## EXAMPLES:


## DOCUMENTATION:

Here is the MediatR library documentation: https://github.com/LuckyPennySoftware/MediatR/wiki

## OTHER CONSIDERATIONS:

 - remove the example weather forecast endpoint from the initial WebApi project
 - Use APS.NET Core minimal API
 - Use Entity Framework Core as ORM with code first approach and migrations for DB upgrade mechanism, seed data can be part of the migrations
 - EF related code like migration and context can be in Dal folder
 - Module related classes go into separate folders per module, common code can go into Common folder
 - Use domain entities also as EF Core entities and use domain driven development approach
 - Endpoints mapping will be in separate classes as an extension method for WebApplication class per each module
 - the endpoint logic will be in separated classes per each enpoint preferable use Meddiator pattern and MediatR library
 - all logic will be implemented in handlers called by MediatR and handle will return IResult for each request
 - implement validation using FluentValidation library
 - implement error handling using custom middleware 
 - for tests create solution folder Tests and in that folder test project for each project of the solution for example for WebApi project will be tested by Tests/WebApi.Tests project
 - test will be implemented using xUnit and FluentAssertions and if needed for moqs use NSubstitute
 - for tests use in memory database provider for EF Core
 - focus on clean code and SOLID principles, YAGNI principle, KISS principle and functional requirements
 - update CLAUDE.md file with any new information you find important