# Step 10 — Running the Project

## Prerequisites

- .NET 8 SDK (installed via `brew install dotnet@8`)
- Docker (Rancher Desktop, Docker Desktop, or OrbStack)
- Node.js 20.19+ or 22+

## Start order

Services must start in this order: **DB → API → Client**

---

### 1. Start the database

**Apple Silicon (Mac M-chip):**
```bash
docker run -d --name fym-mssql \
  -e "ACCEPT_EULA=1" \
  -e "MSSQL_SA_PASSWORD=YourStrong!Passw0rd" \
  -p 1433:1433 \
  mcr.microsoft.com/azure-sql-edge:latest
```

**Linux / Windows / Intel Mac:**
```bash
docker run -d --name fym-mssql \
  -e "ACCEPT_EULA=Y" \
  -e "MSSQL_SA_PASSWORD=YourStrong!Passw0rd" \
  -e "MSSQL_PID=Developer" \
  -p 1433:1433 \
  mcr.microsoft.com/mssql/server:2022-latest
```

Wait ~10 seconds for the engine to initialize before starting the API.

---

### 2. Start the API

```bash
export PATH="/opt/homebrew/opt/dotnet@8/bin:$HOME/.dotnet/tools:$PATH"
export DOTNET_ROOT="/opt/homebrew/opt/dotnet@8/libexec"
cd /Users/mbp/Documents/TEST

dotnet run --project src/FymUsers.Api --urls http://localhost:5080
```

On first boot the API will:
1. Apply EF Core migrations (create the `FymUsers` database and tables).
2. Seed roles (SuperAdmin, Admin, User).
3. Create the `superadmin` user.

**Swagger UI:** http://localhost:5080/swagger

---

### 3. Start the React client

```bash
cd /Users/mbp/Documents/TEST/client
npm install   # only needed on first run
npm run dev
```

**Client:** http://localhost:5173

---

## Seed credentials

| Field | Value |
|-------|-------|
| Username | `superadmin` |
| Password | `SuperAdmin123!` |
| Email | `superadmin@fym.local` |

---

## Quick smoke test

```bash
# Login and capture token
TOKEN=$(curl -s -X POST http://localhost:5080/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"userName":"superadmin","password":"SuperAdmin123!"}' \
  | python3 -c "import sys,json;print(json.load(sys.stdin)['accessToken'])")

# List roles
curl -s http://localhost:5080/api/roles \
  -H "Authorization: Bearer $TOKEN"

# Create a user
curl -s -X POST http://localhost:5080/api/users \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"userName":"alice","email":"alice@example.com","password":"Alice123!@#","roleIds":[3]}'

# List users
curl -s http://localhost:5080/api/users \
  -H "Authorization: Bearer $TOKEN"
```

---

## Resetting the database

```bash
docker rm -f fym-mssql
# Then re-run the docker run command above
# The next API startup recreates everything automatically
```

---

## Services at a glance

| Service | URL | Credentials |
|---------|-----|-------------|
| Swagger | http://localhost:5080/swagger | — |
| API base | http://localhost:5080 | — |
| React client | http://localhost:5173 | — |
| SQL Server | localhost:1433 | sa / YourStrong!Passw0rd |
