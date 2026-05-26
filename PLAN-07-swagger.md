# Step 7 — Swagger

## Goal
Auto-generate interactive API documentation that reviewers can use to test every endpoint without writing any code.

## What Swagger provides

- A web UI at `/swagger` listing every endpoint, its parameters, and its response shapes.
- A built-in HTTP client — fill in the fields and hit "Execute" to send a real request.
- An **Authorize** button to supply the JWT once and have it applied to all subsequent requests.

## JWT integration

Without extra configuration, Swagger has no way to send the `Authorization` header. Two additions fix this:

1. **Security definition** — registers a Bearer scheme so the Authorize button appears.
2. **Security requirement** — marks all endpoints as requiring that scheme by default.

After logging in via `/api/auth/login`, copy the `accessToken` value, click Authorize, paste it (no `Bearer ` prefix needed), and all subsequent Swagger requests are authenticated.

## XML summary comments

Each controller method has a `/// <summary>` comment:

```csharp
/// <summary>Create a new user. Restricted to SuperAdmin.</summary>
[HttpPost]
[Authorize(Roles = RoleNames.SuperAdmin)]
public async Task<ActionResult<UserDto>> Create(...)
```

These appear as descriptions in the Swagger UI next to each endpoint.

## Access

- Swagger UI: http://localhost:5080/swagger
- Raw OpenAPI JSON: http://localhost:5080/swagger/v1/swagger.json
- Root `/` redirects to `/swagger` automatically.

## Key files

- [src/FymUsers.Api/Program.cs](src/FymUsers.Api/Program.cs) — `AddSwaggerGen` and `UseSwaggerUI` configuration
