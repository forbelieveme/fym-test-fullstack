# Step 8 — Database Connection Security

## Goal
Ensure all traffic between the API and SQL Server is encrypted, as required by the test specification.

## Connection string

```
Server=localhost,1433;
Database=FymUsers;
User Id=sa;
Password=YourStrong!Passw0rd;
Encrypt=True;
TrustServerCertificate=True;
```

## What each security flag does

| Flag | Value | Effect |
|------|-------|--------|
| `Encrypt` | `True` | All data between the API and SQL Server travels over TLS — not plain text |
| `TrustServerCertificate` | `True` | Accepts the self-signed certificate the Docker container generates |

## Why `TrustServerCertificate=True` in dev

The `azure-sql-edge` Docker container creates a self-signed TLS certificate on first boot. Self-signed certificates are not issued by a trusted Certificate Authority (CA), so the SQL Server client would normally reject them. Setting `TrustServerCertificate=True` tells the client to skip that CA check.

**This is only acceptable in a local development environment.**

## What to change for production

- Provision a real TLS certificate for the SQL Server instance (from a CA or Let's Encrypt).
- Set `TrustServerCertificate=False` (or remove the flag — it defaults to false).
- Move the password out of `appsettings.json` into a secret store (environment variable, Azure Key Vault, `dotnet user-secrets`).

## Database server

On Apple Silicon (Mac M-chip), SQL Server 2022 segfaults under QEMU emulation. The fallback is `azure-sql-edge`, which is ARM64-native and fully compatible with EF Core and the SQL Server connection driver.

```bash
# Apple Silicon
docker run -d --name fym-mssql \
  -e "ACCEPT_EULA=1" \
  -e "MSSQL_SA_PASSWORD=YourStrong!Passw0rd" \
  -p 1433:1433 \
  mcr.microsoft.com/azure-sql-edge:latest

# Linux / Windows / Intel Mac
docker run -d --name fym-mssql \
  -e "ACCEPT_EULA=Y" \
  -e "MSSQL_SA_PASSWORD=YourStrong!Passw0rd" \
  -e "MSSQL_PID=Developer" \
  -p 1433:1433 \
  mcr.microsoft.com/mssql/server:2022-latest
```

## Key files

- [src/FymUsers.Api/appsettings.json](src/FymUsers.Api/appsettings.json) — connection string lives here
