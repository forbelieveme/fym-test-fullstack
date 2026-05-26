#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")" && pwd)"

echo "[1/3] DB — starting fym-mssql..."
docker start fym-mssql

echo "[2/3] API — restarting on :5080..."
kill "$(lsof -ti:5080)" 2>/dev/null || true
sleep 1
export PATH="/opt/homebrew/opt/dotnet@8/bin:$HOME/.dotnet/tools:$PATH"
export DOTNET_ROOT="/opt/homebrew/opt/dotnet@8/libexec"
dotnet run --project "$ROOT/src/FymUsers.Api" \
  --urls http://localhost:5080 > /tmp/fym-api.log 2>&1 &
API_PID=$!

echo "[3/3] Client — restarting on :5173..."
kill "$(lsof -ti:5173)" 2>/dev/null || true
sleep 1
cd "$ROOT/client" && npm run dev > /tmp/fym-client.log 2>&1 &
CLIENT_PID=$!

echo "Waiting for API..."
for i in $(seq 1 30); do
  curl -sf http://localhost:5080/swagger/index.html > /dev/null 2>&1 && break
  sleep 1
done

echo "Waiting for client..."
for i in $(seq 1 20); do
  curl -sf http://localhost:5173 > /dev/null 2>&1 && break
  sleep 1
done

echo ""
echo "All services running:"
echo "  API     → http://localhost:5080/swagger  (log: /tmp/fym-api.log)"
echo "  Client  → http://localhost:5173          (log: /tmp/fym-client.log)"
echo "  DB      → localhost:1433"
