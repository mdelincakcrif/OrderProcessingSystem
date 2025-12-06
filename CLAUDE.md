# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is an ASP.NET Core Web API project targeting .NET 10.0. The solution is named "OrderProcessingSystem" and currently contains a single WebApi project with minimal configuration (currently featuring a sample weather forecast endpoint).

## Project Structure

- **WebApi/** - Main Web API project
  - `Program.cs` - Application entry point with minimal API configuration
  - `WebApi.csproj` - Project file targeting net10.0
  - `appsettings.json` / `appsettings.Development.json` - Configuration files
  - `Properties/launchSettings.json` - Launch profiles for development

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

### Testing

```bash
# Run all tests (when test projects are added)
dotnet test

# Run tests for a specific project
dotnet test <TestProject>/<TestProject>.csproj

# Run a single test
dotnet test --filter "FullyQualifiedName=<Namespace>.<TestClass>.<TestMethod>"
```

### Other Useful Commands

```bash
# Restore dependencies
dotnet restore

# Clean build artifacts
dotnet clean

# Add a NuGet package
dotnet add WebApi/WebApi.csproj package <PackageName>

# Create a new project in the solution
dotnet new <template> -n <ProjectName>
```

## Architecture Notes

### Current Architecture

The project uses ASP.NET Core minimal APIs with a simple inline endpoint configuration in `Program.cs`. The application is configured with:
- Default logging (Information level for app, Warning for ASP.NET Core)
- Development environment defaults to http://localhost:5153
- Nullable reference types enabled

### When Adding Features

Since this appears to be an early-stage project:
- Consider organizing endpoints into separate files or classes as the API grows
- Add structured logging and configuration as needed
- Implement dependency injection for services through `builder.Services`
- Consider adding authentication/authorization middleware before `app.Run()`
