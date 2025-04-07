# iTracker Application

## Azure Deployment Instructions

This application is configured to deploy to Azure App Service and connect to Azure SQL Database.

### Prerequisites

1. Azure subscription
2. Azure CLI installed
3. .NET 8.0 SDK installed

### Deployment Options

#### Option 1: Manual Deployment using PowerShell Script

1. Build the application:
   ```
   dotnet publish WebApp/WebApp.csproj -c Release -o ./publish
   ```

2. Create a zip file of the publish folder:
   ```
   Compress-Archive -Path ./publish -DestinationPath ./publish.zip
   ```

3. Run the deployment script:
   ```
   ./deploy-to-azure.ps1 -ResourceGroupName "iTracker-RG" -AppServiceName "itracker" -Location "eastus"
   ```

#### Option 2: GitHub Actions Deployment

1. Fork this repository to your GitHub account
2. Create the following secrets in your GitHub repository:
   - `AZURE_WEBAPP_NAME`: Your Azure App Service name
   - `AZURE_WEBAPP_PUBLISH_PROFILE`: Your Azure App Service publish profile (can be downloaded from the Azure portal)

3. Push to the main branch to trigger the deployment

### Azure SQL Database Connection

The application is configured to connect to the following Azure SQL Database:
- Server: itrackersql.database.windows.net
- Database: iTrackerSQLServer
- User: iTrackerDBAdmin

### Environment Configuration

The application uses different configuration files for different environments:
- `appsettings.json`: Default configuration
- `appsettings.Production.json`: Production-specific configuration

### Troubleshooting

If you encounter issues with the deployment:

1. Check the Azure App Service logs in the Azure portal
2. Verify the connection string in the Azure App Service Configuration
3. Ensure the SQL Server firewall allows connections from the App Service IP

### Security Considerations

For production deployments, consider:
1. Using Azure Key Vault to store sensitive information
2. Implementing Azure AD authentication for the SQL Database
3. Setting up proper network security groups and firewall rules 