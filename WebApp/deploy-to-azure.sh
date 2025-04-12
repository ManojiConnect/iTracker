#!/bin/bash

# Set variables for the existing Azure resources
RESOURCE_GROUP="iTrackerResourceGroup"
APP_SERVICE_PLAN="iTrackerAppPlan"
WEBAPP_NAME="iTrackerApp"
CONNECTION_STRING_NAME="DefaultConnection"
CONNECTION_STRING_TYPE="SQLite"
CONNECTION_STRING_VALUE="Data Source=App_Data/itracker.db"

echo "Deploying to existing App Service '$WEBAPP_NAME' in resource group '$RESOURCE_GROUP'..."

# Configure Web App
echo "Configuring Web App..."
az webapp config set --name $WEBAPP_NAME --resource-group $RESOURCE_GROUP --startup-file "dotnet WebApp.dll"

# Set connection string
echo "Setting connection string..."
az webapp config connection-string set --name $WEBAPP_NAME --resource-group $RESOURCE_GROUP --settings $CONNECTION_STRING_NAME="$CONNECTION_STRING_VALUE" --connection-string-type $CONNECTION_STRING_TYPE

# Create deployment package
echo "Creating deployment package..."
cd "$(dirname "$0")"
zip -r deploy.zip publish

# Deploy application
echo "Deploying application..."
az webapp deployment source config-zip --resource-group $RESOURCE_GROUP --name $WEBAPP_NAME --src deploy.zip

echo "Deployment completed successfully!"
echo "Your application is now available at: https://$WEBAPP_NAME.azurewebsites.net" 