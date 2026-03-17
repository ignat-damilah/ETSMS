# Foundation Project Setup

## Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download) (LTS release) installed and available on your PATH.
- SQL Server instance (local or managed) matching the connection string configured in `appsettings.Development.json`.
- Access to Azure AD tenant information (Authority/Audience) if testing authentication.
- Optional: Visual Studio 2022+ or VS Code with the C# extension for debugging and IntelliSense.

## Repository Clone
```bash
git clone https://github.com/<organization>/Foundation.git
cd Foundation
```

## Configuration
1. Copy `appsettings.json` (already committed) and, if needed, create a `appsettings.Development.json` override to store local settings for connection strings and secrets.
2. Update `Authentication:AzureAd` settings with your tenant values:
   ```json
   {
     "Authority": "https://login.microsoftonline.com/<tenant-id>",
     "Audience": "api://<client-id>"
   }
   ```
3. Configure the database connection string under `Infrastructure:Database` to point to your local SQL Server.
4. Store any secrets (such as certificate paths or API keys) in your environment or secure secret store and reference them via configuration.

## Building and Running Locally
```bash
cd src/Foundation.API
dotnet restore
dotnet build
dotnet run
```
- The API exposes health probes at `/health/live`, `/health/ready`, and `/health` as well as a metrics endpoint at `/metrics`.
- Authenticated endpoints require a valid JWT issued by the configured Azure AD tenant.

## Development Tips
- Use `dotnet watch run` for hot reload while iterating.
- Seed data and migrations are managed via EF Core migrations in the `Foundation.Infrastructure` project; run `dotnet ef database update` from that project when schemas change.
- Use `dotnet test` under the `src/Foundation.Application` or other test projects (if added) to validate business logic.

## Troubleshooting
- Check connection availability by running SQL Server locally and ensuring your connection string matches credentials.
- Use the health endpoints and Application Insights telemetry for runtime diagnostics.
- Validate JWT token audiences/issuers with online tools if authentication fails.
