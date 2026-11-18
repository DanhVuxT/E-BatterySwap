#!/bin/bash
# One-time database setup for the local Docker stack.
set -euo pipefail

SQLCMD=/opt/mssql-tools18/bin/sqlcmd

run() {
  "$SQLCMD" -C -b -S sqlserver -U sa -P "$MSSQL_SA_PASSWORD" "$@"
}

echo "[db-init] Creating database E_BatterySwap if it does not exist..."
run -Q "IF DB_ID(N'E_BatterySwap') IS NULL CREATE DATABASE E_BatterySwap;"

HAS_SCHEMA=$(run -d E_BatterySwap -h -1 -W \
  -Q "SET NOCOUNT ON; SELECT COUNT(*) FROM sys.tables WHERE name = N'Role' AND schema_id = SCHEMA_ID(N'dbo');" \
  | tr -d '[:space:]')

if [ "$HAS_SCHEMA" = "0" ]; then
  echo "[db-init] Applying schema.sql..."
  run -d E_BatterySwap -i /sql/schema.sql
else
  echo "[db-init] Schema already exists, skipping schema.sql."
fi

echo "[db-init] Loading seed-data.sql..."
run -i /sql/seed-data.sql

echo "[db-init] Done."
