# iTracker Application

iTracker is a comprehensive investment tracking application built with .NET 8.0, designed to help users manage and monitor their investment portfolios effectively.

## Features

### Investment Management
- Create and manage multiple investment portfolios
- Track investment values over time
- Record and monitor investment performance
- Support for different investment types (stocks, bonds, mutual funds, etc.)
- Historical value tracking with customizable time periods

### Investment History
- Detailed history tracking for each investment
- Monthly value updates
- Performance metrics and analytics
- Edit and delete historical records
- Visual representation of investment trends

### System Settings
- Customizable currency formatting
- Configurable number formatting
- User preferences management
- System-wide settings control

### User Interface
- Modern, responsive web interface
- Interactive data tables
- Modal dialogs for data entry and editing
- Real-time updates
- Mobile-friendly design

## Technical Architecture

### Backend
- Built with .NET 8.0
- Clean Architecture implementation
- CQRS pattern with MediatR
- Entity Framework Core for data access
- Repository pattern for data abstraction
- Dependency Injection for loose coupling

### Frontend
- ASP.NET Core Razor Pages
- Bootstrap 5 for responsive design
- JavaScript for interactive features
- AJAX for asynchronous operations

### Database
- Azure SQL Database
- Entity Framework Core migrations
- Optimized query performance
- Data integrity constraints

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

## Development Guidelines

### Code Style
- Follow C# coding conventions
- Use async/await for asynchronous operations
- Implement proper error handling
- Write unit tests for new features
- Use dependency injection

### Best Practices
- Keep controllers thin
- Use DTOs for data transfer
- Implement proper validation
- Follow SOLID principles
- Write meaningful comments

### Testing
- Unit tests using xUnit
- Integration tests for API endpoints
- UI tests for critical user flows
- Performance testing for database operations

## Troubleshooting

If you encounter issues with the deployment:

1. Check the Azure App Service logs in the Azure portal
2. Verify the connection string in the Azure App Service Configuration
3. Ensure the SQL Server firewall allows connections from the App Service IP

### Common Issues
- Database connection issues
- Deployment failures
- Performance problems
- UI rendering issues

## Security Considerations

For production deployments, consider:
1. Using Azure Key Vault to store sensitive information
2. Implementing Azure AD authentication for the SQL Database
3. Setting up proper network security groups and firewall rules
4. Implementing proper input validation
5. Using HTTPS for all communications
6. Regular security audits

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details. 