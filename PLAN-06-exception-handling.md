# Step 6 — Exception Handling

## Goal
Return consistent, machine-readable error responses for every failure — expected or unexpected.

## Middleware position in the pipeline

```
Request
  → ExceptionHandlingMiddleware   ← wraps everything below
    → Swagger
      → CORS
        → Authentication
          → Authorization
            → Controller
```

Any exception thrown anywhere below gets caught here before ASP.NET's default handler sees it.

## Two exception types

### DomainException (deliberate)
Thrown by controllers when something predictable goes wrong.

| Factory method | HTTP status | When used |
|----------------|------------|-----------|
| `DomainException.NotFound("User")` | 404 | Record doesn't exist |
| `DomainException.Conflict("UserName already in use.")` | 409 | Duplicate unique field |
| `DomainException.BadRequest("...")` | 400 | Invalid input that passed DTO validation |
| `DomainException.Unauthorized()` | 401 | Wrong password |

### Any other Exception (unexpected)
Logged at ERROR level, returned as 500. The detail message is generic so internal information is not leaked to the client.

## Response format — RFC 7807 Problem Details

Every error response has `Content-Type: application/problem+json`:

```json
{
  "title": "Conflict",
  "status": 409,
  "detail": "UserName already in use.",
  "instance": "/api/users"
}
```

This is an industry standard format. Clients can reliably parse errors without guessing the shape.

## Why DB-level uniqueness matters here

The application checks for duplicates with `AnyAsync` before inserting. But two simultaneous requests can both pass that check before either writes. In that race, one insert will violate the DB unique constraint and throw an `SqlException`. The middleware catches it and returns 409 — same response as the deliberate check, no crash, no 500.

## Key files

- [src/FymUsers.Api/Middleware/ExceptionHandlingMiddleware.cs](src/FymUsers.Api/Middleware/ExceptionHandlingMiddleware.cs)
