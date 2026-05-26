# Step 1 — Solution Structure

## Goal
Organize the codebase into projects that each have one clear responsibility.

## Layout

```
FymUsers.sln
src/
  FymUsers.Api/            → HTTP layer: controllers, middleware, JWT config, Swagger
  FymUsers.Domain/         → Business entities only (no framework dependencies)
  FymUsers.Infrastructure/ → Database: EF Core DbContext, migrations
client/                    → React frontend
```

## Project references

- **Api** depends on Domain + Infrastructure
- **Infrastructure** depends on Domain
- **Domain** depends on nothing

## Why three projects?

Separating Domain from Infrastructure means the business rules never depend on the database. Separating Api from Infrastructure means you could swap SQL Server for another database without touching the controllers.

## Commands used

```bash
dotnet new sln -n FymUsers
dotnet new webapi -n FymUsers.Api -o src/FymUsers.Api --use-controllers --framework net8.0
dotnet new classlib -n FymUsers.Domain -o src/FymUsers.Domain --framework net8.0
dotnet new classlib -n FymUsers.Infrastructure -o src/FymUsers.Infrastructure --framework net8.0
dotnet sln add src/FymUsers.Api src/FymUsers.Domain src/FymUsers.Infrastructure
```

## Key files

- [src/FymUsers.Api/Program.cs](src/FymUsers.Api/Program.cs)
- [src/FymUsers.Api/FymUsers.Api.csproj](src/FymUsers.Api/FymUsers.Api.csproj)
- [src/FymUsers.Domain/FymUsers.Domain.csproj](src/FymUsers.Domain/FymUsers.Domain.csproj)
- [src/FymUsers.Infrastructure/FymUsers.Infrastructure.csproj](src/FymUsers.Infrastructure/FymUsers.Infrastructure.csproj)
