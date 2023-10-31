#!/bin/sh

echo You are about to generate a new database migration for Sqlite, SQLServer, Postgres etc.
echo What would you like to name this migration?
read -p 'Migration name: ' migrationName

export DATABASE_PROVIDER=SQLite
echo Generating $DATABASE_PROVIDER master database migrations...
dotnet ef migrations add $migrationName --context MasterSqliteDb --output-dir Migrations/MasterDb/SqliteMigrations

export DATABASE_PROVIDER=SqlServer
echo Generating $DATABASE_PROVIDER master database migrations...
dotnet ef migrations add $migrationName --context MasterSqlServerDb --output-dir Migrations/MasterDb/SqlServerMigrations

export DATABASE_PROVIDER=Postgres
echo Generating $DATABASE_PROVIDER master database migrations...
dotnet ef migrations add $migrationName --context MasterPostgresDb --output-dir Migrations/MasterDb/PostgresMigrations