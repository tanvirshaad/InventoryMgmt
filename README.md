# Inventory Management System

## Environment Setup

This application uses environment variables for sensitive configuration. Follow these steps to set it up:

1. Copy `.env.example` to `.env` at the root of the project
2. Fill in your actual Cloudinary credentials and JWT settings in the `.env` file:

```
# JWT Settings
JWT_KEY=YourSuperSecretKeyHere12345678901234567890
JWT_ISSUER=InventoryMgmt
JWT_AUDIENCE=InventoryMgmtUsers
JWT_EXPIRY_DAYS=7

# Cloudinary Settings
CLOUDINARY_CLOUD_NAME=your_cloud_name_here
CLOUDINARY_API_KEY=your_api_key_here
CLOUDINARY_API_SECRET=your_api_secret_here
```

3. Make sure to keep the `.env` file secure and never commit it to source control

## Getting Started

1. Ensure you have .NET 8.0 SDK installed
2. Set up your database connection in `appsettings.json`
3. Run the application: `dotnet run --project InventoryMgmt.MVC/InventoryMgmt.MVC.csproj`

## Features

- Inventory Management
- Item Tracking
- User Authentication
- Image Upload (via Cloudinary)
- Custom Fields
- Custom ID Generation
- Access Control
