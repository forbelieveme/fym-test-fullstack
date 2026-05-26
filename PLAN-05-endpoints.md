# Step 5 — API Endpoints

## Goal
Expose the minimum set of routes required by the test specification.

## Endpoint table

| Method | Route | Auth required | Description |
|--------|-------|--------------|-------------|
| POST | `/api/auth/login` | None | Authenticate and receive a JWT |
| GET | `/api/users` | Any logged-in user | List all users with their roles |
| GET | `/api/users/me` | Any logged-in user | Your own profile |
| GET | `/api/users/{id}` | Any logged-in user | One user by ID |
| POST | `/api/users` | SuperAdmin only | Create a new user |
| POST | `/api/users/{id}/roles` | SuperAdmin only | Assign roles to a user |
| GET | `/api/roles` | Any logged-in user | List all roles |

## Authorization layers

```
[Authorize]                    → valid JWT required (all endpoints except login)
[Authorize(Roles="SuperAdmin")] → additionally requires SuperAdmin role claim
```

A regular user hitting a SuperAdmin endpoint gets **403 Forbidden** — not 404. The endpoint exists; they just don't have permission.

## DTOs (request/response shapes)

### POST /api/auth/login
Request:
```json
{ "userName": "superadmin", "password": "SuperAdmin123!" }
```
Response:
```json
{
  "accessToken": "eyJ...",
  "expiresAtUtc": "2026-05-26T15:42:41Z",
  "user": { "id": 1, "userName": "superadmin", "email": "...", "roles": [...] }
}
```

### POST /api/users (SuperAdmin only)
Request:
```json
{
  "userName": "alice",
  "email": "alice@example.com",
  "password": "Alice123!@#",
  "roleIds": [3]
}
```

### POST /api/users/{id}/roles (SuperAdmin only)
Request:
```json
{ "roleIds": [2, 3] }
```

## Validation rules

- `UserName`: required, 3–64 chars
- `Email`: required, valid email format, max 256 chars
- `Password`: required, min 8 chars
- `roleIds`: must reference existing role IDs — validated against DB before insert

## Key files

- [src/FymUsers.Api/Controllers/AuthController.cs](src/FymUsers.Api/Controllers/AuthController.cs)
- [src/FymUsers.Api/Controllers/UsersController.cs](src/FymUsers.Api/Controllers/UsersController.cs)
- [src/FymUsers.Api/Controllers/RolesController.cs](src/FymUsers.Api/Controllers/RolesController.cs)
- [src/FymUsers.Api/Dtos/AuthDtos.cs](src/FymUsers.Api/Dtos/AuthDtos.cs)
