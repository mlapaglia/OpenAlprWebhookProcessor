#!/bin/sh
set -e

echo "OpenAlprWebhookProcessor starting..."

APPSETTINGS_FILE="appsettings.json"
if [ -f "appsettings.Production.json" ]; then
    APPSETTINGS_FILE="appsettings.Production.json"
fi

echo "Reading connection strings from $APPSETTINGS_FILE..."

PROCESSOR_CONNECTION=$(jq -r '.ConnectionStrings.ProcessorConnection // empty' "$APPSETTINGS_FILE")
USERS_CONNECTION=$(jq -r '.ConnectionStrings.UsersConnection // empty' "$APPSETTINGS_FILE")

if [ -z "$PROCESSOR_CONNECTION" ]; then
    echo "ERROR: Could not find ProcessorConnection in $APPSETTINGS_FILE"
    exit 1
fi

if [ -z "$USERS_CONNECTION" ]; then
    echo "ERROR: Could not find UsersConnection in $APPSETTINGS_FILE"
    exit 1
fi

echo "Processor connection: $PROCESSOR_CONNECTION"
echo "Users connection: $USERS_CONNECTION"

ensure_db_directory() {
    local connection_string="$1"
    local db_path=$(echo "$connection_string" | sed -n 's/.*Data Source=\([^;]*\).*/\1/p')
    local db_dir=$(dirname "$db_path")
    
    if [ ! -d "$db_dir" ]; then
        echo "Creating database directory: $db_dir"
        mkdir -p "$db_dir"
    fi
}

ensure_db_directory "$PROCESSOR_CONNECTION"
ensure_db_directory "$USERS_CONNECTION"

echo "Running migrations for ProcessorConnection..."
./processor-migrator --connection "$PROCESSOR_CONNECTION" 2>&1

MIGRATION_EXIT_CODE=$?
if [ $MIGRATION_EXIT_CODE -eq 0 ]; then
    echo "ProcessorConnection migrations completed successfully"
else
    echo "ERROR: ProcessorConnection migration failed with exit code $MIGRATION_EXIT_CODE"
    exit $MIGRATION_EXIT_CODE
fi

echo "Running migrations for UsersConnection..."
./users-migrator --connection "$USERS_CONNECTION" 2>&1

MIGRATION_EXIT_CODE=$?
if [ $MIGRATION_EXIT_CODE -eq 0 ]; then
    echo "UsersConnection migrations completed successfully"
else
    echo "ERROR: UsersConnection migration failed with exit code $MIGRATION_EXIT_CODE"
    exit $MIGRATION_EXIT_CODE
fi

echo "All migrations completed successfully"
echo "Starting application..."
exec dotnet OpenAlprWebhookProcessor.Server.dll