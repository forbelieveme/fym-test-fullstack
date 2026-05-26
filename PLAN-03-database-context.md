# Step 3 — Database Context and Seed Data

## Goal
Configure EF Core to map the entities to SQL Server tables, enforce constraints, and populate initial data.

## AppDbContext responsibilities

- Maps `User`, `Role`, `UserRole` to their tables.
- Configures unique indexes on `UserName` and `Email` at the database level.
- Sets cascade delete on `User → UserRoles` (deleting a user removes their role assignments).
- Sets restrict delete on `Role → UserRoles` (cannot delete a role while users hold it).
- Seeds the three roles with stable integer IDs via `HasData`.

## Why unique indexes at the DB level?
Application code checks for duplicates before inserting, but two simultaneous requests can both pass the check before either one writes. The database constraint closes that gap — it enforces uniqueness atomically. See [PLAN-06-exception-handling.md](PLAN-06-exception-handling.md) for how the resulting error is caught and returned as a 409.

## Seeded roles

| Id | Name | Description |
|----|------|-------------|
| 1 | SuperAdmin | Full system access; can create users |
| 2 | Admin | Manages users and roles |
| 3 | User | Standard authenticated user |

## DbSeeder (runs at API startup)

1. Calls `MigrateAsync()` — applies any pending EF migrations automatically.
2. Checks if `superadmin` user exists; if not, creates it.
3. Hashes the seed password with BCrypt at runtime (work factor 11).
4. Assigns the SuperAdmin role.

**Why seed the user at startup instead of in the migration?**
`HasData` in migrations requires static values baked into the migration file. BCrypt hashes include a random salt generated at runtime — they cannot be predetermined. Seeding at startup lets us call `BCrypt.HashPassword()` properly.

## Seed credentials

- Username: `superadmin`
- Password: `SuperAdmin123!`
- Email: `superadmin@fym.local`

## Key files

- [src/FymUsers.Infrastructure/Persistence/AppDbContext.cs](src/FymUsers.Infrastructure/Persistence/AppDbContext.cs)
- [src/FymUsers.Api/Services/DbSeeder.cs](src/FymUsers.Api/Services/DbSeeder.cs)
- [src/FymUsers.Infrastructure/Persistence/Migrations/](src/FymUsers.Infrastructure/Persistence/Migrations/)

## Migration commands

```bash
export PATH="/opt/homebrew/opt/dotnet@8/bin:$HOME/.dotnet/tools:$PATH"
export DOTNET_ROOT="/opt/homebrew/opt/dotnet@8/libexec"
cd /Users/mbp/Documents/TEST

dotnet ef migrations add InitialCreate \
  --project src/FymUsers.Infrastructure \
  --startup-project src/FymUsers.Api \
  -o Persistence/Migrations

dotnet ef database update \
  --project src/FymUsers.Infrastructure \
  --startup-project src/FymUsers.Api
```
