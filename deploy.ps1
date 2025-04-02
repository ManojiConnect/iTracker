# Deployment script for iTracker application

# Azure resource details
$resourceGroup = "iTrackerResourceGroup"
$webAppName = "iTrackerApp"
$deployZipPath = "deploy.zip"

# Deploy the application
Write-Host "Deploying to Azure Web App: $webAppName"
az webapp deploy --resource-group $resourceGroup --name $webAppName --src-path $deployZipPath --type zip

Write-Host "Deployment completed"
Write-Host "You can access the application at: http://$webAppName.azurewebsites.net"
Write-Host "Admin password reset page is available at: http://$webAppName.azurewebsites.net/ResetPassword" 