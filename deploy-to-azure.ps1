# Azure App Service deployment script
param(
    [Parameter(Mandatory=$true)]
    [string]$ResourceGroupName,
    
    [Parameter(Mandatory=$true)]
    [string]$AppServiceName,
    
    [Parameter(Mandatory=$true)]
    [string]$Location = "eastus"
)

# Ensure Azure CLI is installed and logged in
Write-Host "Checking Azure CLI installation..." -ForegroundColor Cyan
$azVersion = az version --output json | ConvertFrom-Json
if (-not $azVersion) {
    Write-Host "Azure CLI is not installed. Please install it first." -ForegroundColor Red
    exit 1
}

# Login to Azure if not already logged in
$account = az account show --output json | ConvertFrom-Json
if (-not $account) {
    Write-Host "Logging in to Azure..." -ForegroundColor Cyan
    az login
}

# Create resource group if it doesn't exist
Write-Host "Creating resource group '$ResourceGroupName' in '$Location'..." -ForegroundColor Cyan
az group create --name $ResourceGroupName --location $Location

# Create App Service plan
$appServicePlanName = "$AppServiceName-plan"
Write-Host "Creating App Service plan '$appServicePlanName'..." -ForegroundColor Cyan
az appservice plan create --name $appServicePlanName --resource-group $ResourceGroupName --location $Location --sku B1 --is-linux

# Create Web App
Write-Host "Creating Web App '$AppServiceName'..." -ForegroundColor Cyan
az webapp create --name $AppServiceName --resource-group $ResourceGroupName --plan $appServicePlanName --runtime "DOTNETCORE:8.0"

# Configure Web App
Write-Host "Configuring Web App..." -ForegroundColor Cyan
az webapp config set --name $AppServiceName --resource-group $ResourceGroupName --linux-fx-version "DOTNETCORE|8.0"

# Set connection string
Write-Host "Setting connection string..." -ForegroundColor Cyan
$connectionString = "Server=tcp:itrackersql.database.windows.net,1433;Initial Catalog=iTrackerSQLServer;Persist Security Info=False;User ID=iTrackerDBAdmin;Password=iTracker@2025;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
az webapp config connection-string set --name $AppServiceName --resource-group $ResourceGroupName --settings DefaultConnection="$connectionString" --connection-string-type SQLAzure

# Deploy the application
Write-Host "Deploying application..." -ForegroundColor Cyan
az webapp deployment source config-zip --name $AppServiceName --resource-group $ResourceGroupName --src ./publish.zip

Write-Host "Deployment completed successfully!" -ForegroundColor Green
Write-Host "Your application is now available at: https://$AppServiceName.azurewebsites.net" -ForegroundColor Green 