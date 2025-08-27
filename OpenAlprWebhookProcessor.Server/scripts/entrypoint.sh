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
    local db_path=$(echo "$connection_string" | sed -n 's/.*[Dd]ata[[:space:]]*[Ss]ource=\([^;]*\).*/\1/p')
    db_path=$(echo "$db_path" | sed 's/^"\(.*\)"$/\1/' | sed "s/^'\(.*\)'$/\1/")
    
    if [ -n "$db_path" ] && [ "$db_path" != ":memory:" ]; then
        local db_dir=$(dirname "$db_path")
        
        if [ ! -d "$db_dir" ] && [ "$db_dir" != "." ]; then
            echo "Creating database directory: $db_dir"
            mkdir -p "$db_dir"
        fi
    fi
}

run_sqlite_migrations() {
    local connection_string="$1"
    local script_file="$2"
    local context_name="$3"
    
    # Extract the database path from connection string
    local db_path=$(echo "$connection_string" | sed -n 's/.*[Dd]ata[[:space:]]*[Ss]ource=\([^;]*\).*/\1/p')
    db_path=$(echo "$db_path" | sed 's/^"\(.*\)"$/\1/' | sed "s/^'\(.*\)'$/\1/")
    
    if [ ! -f "$script_file" ]; then
        echo "ERROR: Migration script $script_file not found"
        return 1
    fi
    
    echo "Running $context_name migrations from $script_file..."
    
    # Check if sqlite3 is available
    if ! command -v sqlite3 >/dev/null 2>&1; then
        echo "ERROR: sqlite3 command not found. Please ensure it's installed in the Docker image."
        return 1
    fi
    
    # Run the migration
    sqlite3 -bail "$db_path" < "$script_file" 2>&1
    local exit_code=$?
    
    if [ $exit_code -eq 0 ]; then
        echo "✓ $context_name migrations completed successfully"
        return 0
    else
        echo "✗ ERROR: $context_name migration failed with exit code $exit_code"
        return $exit_code
    fi
}

# Ensure database directories exist
ensure_db_directory "$PROCESSOR_CONNECTION"
ensure_db_directory "$USERS_CONNECTION"

# Run migrations
if [ -f "./processor-migrations.sql" ]; then
    run_sqlite_migrations "$PROCESSOR_CONNECTION" "./processor-migrations.sql" "ProcessorContext"
    MIGRATION_EXIT_CODE=$?
    if [ $MIGRATION_EXIT_CODE -ne 0 ]; then
        exit $MIGRATION_EXIT_CODE
    fi
else
    echo "WARNING: processor-migrations.sql not found, skipping ProcessorContext migrations"
fi

if [ -f "./users-migrations.sql" ]; then
    run_sqlite_migrations "$USERS_CONNECTION" "./users-migrations.sql" "UsersContext"
    MIGRATION_EXIT_CODE=$?
    if [ $MIGRATION_EXIT_CODE -ne 0 ]; then
        exit $MIGRATION_EXIT_CODE
    fi
else
    echo "WARNING: users-migrations.sql not found, skipping UsersContext migrations"
fi

echo "All migrations completed successfully"
echo "Starting application..."
exec dotnet OpenAlprWebhookProcessor.Server.dll