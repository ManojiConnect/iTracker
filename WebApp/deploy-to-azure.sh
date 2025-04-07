#!/bin/bash

# Azure App Service deployment script
RESOURCE_GROUP_NAME="itracker-rg"
APP_SERVICE_NAME="itracker-app"
LOCATION="eastus"

# Create resource group if it doesn't exist
echo "Creating resource group '$RESOURCE_GROUP_NAME' in '$LOCATION'..."
az group create --name $RESOURCE_GROUP_NAME --location $LOCATION

# Create App Service plan
APP_SERVICE_PLAN_NAME="$APP_SERVICE_NAME-plan"
echo "Creating App Service plan '$APP_SERVICE_PLAN_NAME'..."
az appservice plan create --name $APP_SERVICE_PLAN_NAME --resource-group $RESOURCE_GROUP_NAME --location $LOCATION --sku B1 --is-linux

# Create Web App
echo "Creating Web App '$APP_SERVICE_NAME'..."
az webapp create --name $APP_SERVICE_NAME --resource-group $RESOURCE_GROUP_NAME --plan $APP_SERVICE_PLAN_NAME --runtime "DOTNETCORE:8.0"

# Configure Web App
echo "Configuring Web App..."
az webapp config set --name $APP_SERVICE_NAME --resource-group $RESOURCE_GROUP_NAME --linux-fx-version "DOTNETCORE|8.0"

# Set connection string
echo "Setting connection string..."
CONNECTION_STRING="Server=tcp:itrackersql.database.windows.net,1433;Initial Catalog=iTrackerSQLServer;Persist Security Info=False;User ID=iTrackerDBAdmin;Password=iTracker@2025;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
az webapp config connection-string set --name $APP_SERVICE_NAME --resource-group $RESOURCE_GROUP_NAME --settings DefaultConnection="$CONNECTION_STRING" --connection-string-type SQLAzure

# Create deployment package
echo "Creating deployment package..."
cd publish
zip -r ../publish.zip .
cd ..

# Deploy the application
echo "Deploying application..."
az webapp deployment source config-zip --name $APP_SERVICE_NAME --resource-group $RESOURCE_GROUP_NAME --src ./publish.zip

echo "Deployment completed successfully!"
echo "Your application is now available at: https://$APP_SERVICE_NAME.azurewebsites.net" 