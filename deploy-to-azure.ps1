# Azure App Service deployment script for iTracker
param(
    [Parameter(Mandatory=$false)]
    [string]$ResourceGroupName = "iTrackerResourceGroup",
    
    [Parameter(Mandatory=$false)]
    [string]$AppServiceName = "iTrackerApp",
    
    [Parameter(Mandatory=$false)]
    [string]$Location = "eastus",
    
    [Parameter(Mandatory=$false)]
    [string]$AppServicePlanName = "iTrackerAppPlan",
    
    [Parameter(Mandatory=$false)]
    [string]$SqlServerName = "itrackersql",
    
    [Parameter(Mandatory=$false)]
    [string]$SqlDatabaseName = "iTrackerSQLServer"
)

$ErrorActionPreference = "Stop"

# Ensure Azure CLI is installed and logged in
Write-Host "Checking Azure CLI installation..." -ForegroundColor Cyan
try {
    $azVersion = az version --output json | ConvertFrom-Json
    if (-not $azVersion) {
        Write-Host "Azure CLI is not installed. Please install it first." -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "Azure CLI is not installed or not working. Please install it first." -ForegroundColor Red
    exit 1
}

# Login to Azure if not already logged in
Write-Host "Checking Azure login status..." -ForegroundColor Cyan
try {
    $account = az account show --output json | ConvertFrom-Json
    if (-not $account) {
        Write-Host "Logging in to Azure..." -ForegroundColor Cyan
        az login
    } else {
        Write-Host "Already logged in as $($account.user.name)" -ForegroundColor Green
    }
} catch {
    Write-Host "Logging in to Azure..." -ForegroundColor Cyan
    az login
}

# Build and publish the application
Write-Host "Building and publishing the application..." -ForegroundColor Cyan
$publishFolder = "publish"
if (Test-Path $publishFolder) {
    Remove-Item -Path $publishFolder -Recurse -Force
}

Write-Host "Publishing WebApp project..." -ForegroundColor Cyan
dotnet publish WebApp/WebApp.csproj -c Release -o $publishFolder

# Check if publish was successful
if (-not (Test-Path "$publishFolder/WebApp.dll")) {
    Write-Host "Build failed. Could not find WebApp.dll in the publish folder." -ForegroundColor Red
    exit 1
}

# Create a deployment package
Write-Host "Creating deployment package..." -ForegroundColor Cyan
$deploymentPackage = "publish.zip"
if (Test-Path $deploymentPackage) {
    Remove-Item -Path $deploymentPackage -Force
}

# Create the zip file
Compress-Archive -Path "$publishFolder/*" -DestinationPath $deploymentPackage
if (-not (Test-Path $deploymentPackage)) {
    Write-Host "Failed to create deployment package ($deploymentPackage)." -ForegroundColor Red
    exit 1
}

# Create or check resource group
Write-Host "Checking resource group '$ResourceGroupName' in '$Location'..." -ForegroundColor Cyan
$rgExists = az group exists --name $ResourceGroupName
if ($rgExists -eq "false") {
    Write-Host "Creating resource group '$ResourceGroupName' in '$Location'..." -ForegroundColor Cyan
    az group create --name $ResourceGroupName --location $Location
} else {
    Write-Host "Resource group '$ResourceGroupName' already exists." -ForegroundColor Green
}

# Check if App Service Plan exists
Write-Host "Checking App Service plan '$AppServicePlanName'..." -ForegroundColor Cyan
$appServicePlanExists = az appservice plan show --name $AppServicePlanName --resource-group $ResourceGroupName --query name 2>$null
if (-not $appServicePlanExists) {
    # Create App Service plan
    Write-Host "Creating App Service plan '$AppServicePlanName'..." -ForegroundColor Cyan
    az appservice plan create --name $AppServicePlanName --resource-group $ResourceGroupName --location $Location --sku B1 --is-linux
} else {
    Write-Host "App Service plan '$AppServicePlanName' already exists." -ForegroundColor Green
}

# Check if Web App exists
Write-Host "Checking Web App '$AppServiceName'..." -ForegroundColor Cyan
$webAppExists = az webapp show --name $AppServiceName --resource-group $ResourceGroupName --query name 2>$null
if (-not $webAppExists) {
    # Create Web App
    Write-Host "Creating Web App '$AppServiceName'..." -ForegroundColor Cyan
    az webapp create --name $AppServiceName --resource-group $ResourceGroupName --plan $AppServicePlanName --runtime "DOTNETCORE:8.0"
} else {
    Write-Host "Web App '$AppServiceName' already exists." -ForegroundColor Green
}

# Configure Web App
Write-Host "Configuring Web App..." -ForegroundColor Cyan
az webapp config set --name $AppServiceName --resource-group $ResourceGroupName --linux-fx-version "DOTNETCORE|8.0" --always-on true

# Set application settings
Write-Host "Setting application settings..." -ForegroundColor Cyan
az webapp config appsettings set --name $AppServiceName --resource-group $ResourceGroupName --settings ASPNETCORE_ENVIRONMENT="Production"

# Set connection string
Write-Host "Setting connection string..." -ForegroundColor Cyan
$connectionString = "Server=tcp:itrackersql.database.windows.net,1433;Initial Catalog=iTrackerSQLServer;Persist Security Info=False;User ID=iTrackerDBAdmin;Password=iTracker@2025;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
az webapp config connection-string set --name $AppServiceName --resource-group $ResourceGroupName --settings DefaultConnection="$connectionString" --connection-string-type SQLAzure

# Deploy the application
Write-Host "Deploying application..." -ForegroundColor Cyan
az webapp deployment source config-zip --name $AppServiceName --resource-group $ResourceGroupName --src $deploymentPackage

# Configure logging
Write-Host "Configuring Web App logging..." -ForegroundColor Cyan
az webapp log config --name $AppServiceName --resource-group $ResourceGroupName --application-logging filesystem --detailed-error-messages true --failed-request-tracing true --web-server-logging filesystem

# Set CORS policy if needed
# az webapp cors add --name $AppServiceName --resource-group $ResourceGroupName --allowed-origins "https://yourdomain.com"

# Restart the web app to apply all changes
Write-Host "Restarting the Web App to apply changes..." -ForegroundColor Cyan
az webapp restart --name $AppServiceName --resource-group $ResourceGroupName

# Get the deployed app URL
$webAppUrl = az webapp show --name $AppServiceName --resource-group $ResourceGroupName --query defaultHostName --output tsv
Write-Host "Deployment completed successfully!" -ForegroundColor Green
Write-Host "Your application is now available at: https://$webAppUrl" -ForegroundColor Green
Write-Host "Connection string is configured to use Azure SQL Server." -ForegroundColor Green 