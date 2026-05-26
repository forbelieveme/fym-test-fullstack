# Step 4 — JWT Authentication

## Goal
Issue signed tokens on login and validate them on every subsequent request.

## Flow

```
1. POST /api/auth/login  { userName, password }
       ↓
2. Find user in DB by userName
3. Verify BCrypt hash matches password
4. Build JWT with user claims
5. Return token + expiry + user profile
       ↓
6. Client stores token, sends it as:
   Authorization: Bearer <token>
       ↓
7. ASP.NET Core validates token automatically on every request
```

## Token contents (claims)

| Claim | Value | Example |
|-------|-------|---------|
| `sub` | User ID | `1` |
| `unique_name` | Username | `superadmin` |
| `email` | Email | `superadmin@fym.local` |
| `role` | Role name (one per role) | `SuperAdmin` |
| `jti` | Unique token ID (prevents replay) | `89c58ca5-...` |
| `nbf` | Not valid before | Unix timestamp |
| `exp` | Expires at | Unix timestamp |

## Configuration (appsettings.json)

```json
"Jwt": {
  "Issuer": "FymUsers.Api",
  "Audience": "FymUsers.Client",
  "SigningKey": "<at least 32 character secret — change before shipping>",
  "ExpiryMinutes": 60
}
```

## Validation rules enforced on every request

- Signature matches the signing key (HMAC-SHA256).
- `iss` (issuer) matches `FymUsers.Api`.
- `aud` (audience) matches `FymUsers.Client`.
- Token is not expired (`exp` claim).
- Clock skew tolerance: 1 minute.

## Security note
The signing key in `appsettings.json` is a placeholder. In production it must come from a secret store (environment variable, Azure Key Vault, `dotnet user-secrets`) — never committed to source control.

## Key files

- [src/FymUsers.Api/Services/JwtTokenService.cs](src/FymUsers.Api/Services/JwtTokenService.cs)
- [src/FymUsers.Api/Controllers/AuthController.cs](src/FymUsers.Api/Controllers/AuthController.cs)
- [src/FymUsers.Api/appsettings.json](src/FymUsers.Api/appsettings.json)
