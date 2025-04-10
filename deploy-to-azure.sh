#!/bin/bash

# Azure deployment script for iTracker
echo "Starting Azure deployment for iTracker..."

# Configuration
RESOURCE_GROUP="iTrackerResourceGroup"
APP_NAME="iTrackerApp"
PUBLISH_FOLDER="publish"
PROJECT_NAME="WebApp"

# Ensure we're in the right directory
cd "$(dirname "$0")"

# Clean previous publish folder
echo "Cleaning previous publish folder..."
rm -rf $PUBLISH_FOLDER

# Restore and publish the application
echo "Publishing application..."
dotnet publish $PROJECT_NAME/$PROJECT_NAME.csproj -c Release -o $PUBLISH_FOLDER

# Create deployment package
echo "Creating deployment package..."
cd $PUBLISH_FOLDER
zip -r ../deploy.zip ./*
cd ..

# Deploy to Azure
echo "Deploying to Azure..."
az webapp deployment source config-zip \
    --resource-group $RESOURCE_GROUP \
    --name $APP_NAME \
    --src deploy.zip

# Clean up
echo "Cleaning up..."
rm deploy.zip

echo "Deployment completed!"
echo "Your application is available at: https://$APP_NAME.azurewebsites.net" 