#!/bin/bash

# Set variables
RESOURCE_GROUP="iTrackerResourceGroup"
APP_SERVICE_NAME="iTrackerApp"
LOCATION="eastus"

# Create deployment package
cd publish && zip -r ../publish.zip . && cd ..

# Deploy the application
az webapp deploy \
  --name $APP_SERVICE_NAME \
  --resource-group $RESOURCE_GROUP \
  --src-path publish.zip

echo "Deployment completed successfully!"
echo "Your application is now available at: https://itrackerapp.azurewebsites.net" 