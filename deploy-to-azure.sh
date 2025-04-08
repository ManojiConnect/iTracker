#!/bin/bash

# Set variables
RESOURCE_GROUP="iTrackerResourceGroup"
APP_SERVICE_NAME="iTrackerApp"
LOCATION="eastus"

# Set the connection string for SQL Azure database
az webapp config connection-string set \
  --resource-group $RESOURCE_GROUP \
  --name $APP_SERVICE_NAME \
  --settings DefaultConnection="Server=tcp:itracker.database.windows.net,1433;Initial Catalog=iTracker;Persist Security Info=False;User ID=iTrackerDBAdmin;Password=iTracker@2025;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;" \
  --connection-string-type SQLAzure

# Create deployment package
cd publish && zip -r ../publish.zip . && cd ..

# Deploy the application
az webapp deploy \
  --name $APP_SERVICE_NAME \
  --resource-group $RESOURCE_GROUP \
  --src-path publish.zip

echo "Deployment completed successfully!"
echo "Your application is now available at: https://itrackerapp.azurewebsites.net" 