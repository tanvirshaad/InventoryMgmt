# InventoryMgmt.MVC - Enterprise Inventory Management System

![.NET](https://img.shields.io/badge/.NET-8.0-blue)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-8.0-blue)
![Entity Framework](https://img.shields.io/badge/Entity%20Framework-8.0-blue)
![License](https://img.shields.io/badge/license-MIT-green)

A comprehensive, enterprise-grade inventory management system built with ASP.NET Core 8.0, featuring advanced customization, real-time collaboration, and third-party integrations.

## 📋 Table of Contents
- [Application Overview](#-application-overview)
- [Architecture & Design Patterns](#-architecture--design-patterns)
- [Technology Stack](#-technology-stack)
- [Core Features](#-core-features)
- [Project Structure](#-project-structure)
- [Design Principles](#-design-principles)
- [Security Features](#-security-features)
- [Integration Capabilities](#-integration-capabilities)
- [Database Design](#-database-design)
- [Getting Started](#-getting-started)
- [Configuration](#-configuration)
- [API Documentation](#-api-documentation)

## 🚀 Application Overview

**InventoryMgmt.MVC** is a multi-tenant inventory management platform that enables organizations to efficiently manage their inventory collections with advanced customization capabilities, user collaboration features, and seamless third-party integrations.

### What It Is
- 📦 Multi-tenant inventory management platform
- 🔒 Support for both public and private inventory collections
- ⚙️ Advanced custom field configuration for different item types
- 💬 Real-time collaboration with comments and notifications
- 👥 Role-based access control with granular permissions
- 🔗 RESTful API access for external integrations

### How It Works
1. **User Registration & Authentication**: Users register via email/password or OAuth (Google)
2. **Inventory Creation**: Create inventories with customizable field structures
3. **Item Management**: Add items with custom fields, images, and metadata
4. **Collaboration**: Share inventories with specific users or make them public
5. **Real-time Communication**: Use SignalR for live comments and updates
6. **External Integration**: Connect with Salesforce and cloud storage services

## 🏗️ Architecture & Design Patterns

### N-Tier Architecture (3-Layer)

```
┌─────────────────────────────────────┐
│     Presentation Layer (MVC)        │
│  Controllers │ Views │ Hubs │ Models│
├─────────────────────────────────────┤
│      Business Logic Layer (BLL)     │
│  Services │ Interfaces │ DTOs │ Maps│
├─────────────────────────────────────┤
│      Data Access Layer (DAL)        │
│ Repositories │ Entities │ DbContext │
└─────────────────────────────────────┘
```

#### 1. **Presentation Layer (InventoryMgmt.MVC)**
- **Controllers**: Handle HTTP requests and responses
- **Views**: Razor views for UI rendering
- **Models**: ViewModels for data binding
- **Hubs**: SignalR hubs for real-time communication
- **Attributes**: Custom authorization attributes

#### 2. **Business Logic Layer (InventoryMgmt.BLL)**
- **Services**: Encapsulate business logic and rules
- **Interfaces**: Define contracts for dependency injection
- **DTOs**: Data Transfer Objects for data exchange
- **Profiles**: AutoMapper configuration for object mapping

#### 3. **Data Access Layer (InventoryMgmt.DAL)**
- **Repositories**: Abstract data access operations
- **Entity Models**: EF Core entity definitions
- **DbContext**: Database context configuration
- **Migrations**: Database schema versioning

### 🎯 Design Patterns Implemented

| Pattern | Implementation | Purpose |
|---------|---------------|---------|
| **Repository Pattern** | `IRepo<T>` interface with generic CRUD operations | Data access abstraction |
| **Unit of Work Pattern** | `DataAccess` class coordinating repositories | Transaction consistency |
| **Service Layer Pattern** | Business logic in service classes | Separation of concerns |
| **DTO Pattern** | Data Transfer Objects for clean data exchange | Entity encapsulation |
| **Dependency Injection** | Constructor injection throughout layers | Loose coupling |
| **Factory Pattern** | AutoMapper and custom field processing | Object creation |

## 💻 Technology Stack

### Backend Technologies
- **ASP.NET Core 8.0** - Modern web framework
- **Entity Framework Core 8.0** - ORM for database operations
- **SQL Server** - Primary database
- **AutoMapper 15.0.1** - Object-to-object mapping
- **SignalR** - Real-time web functionality

### Authentication & Security
- **JWT Tokens** - API authentication
- **Cookie Authentication** - Web authentication
- **Google OAuth 2.0** - Social login
- **HTTPS** - Secure communication
- **CSRF Protection** - Cross-site request forgery prevention

### Cloud & External Services
- **Cloudinary** - Image storage and processing
- **Salesforce API** - CRM integration
- **Dropbox API** - Document storage
- **OneDrive API** - Alternative document storage

### Frontend Technologies
- **Razor Views** - Server-side rendering
- **Bootstrap 5** - CSS framework
- **JavaScript/jQuery** - Client-side functionality
- **Bootstrap Icons** - Icon library
- **SignalR Client** - Real-time updates

## ✨ Core Features

### 👤 User Management
- ✅ Registration/Login with email-based and OAuth authentication
- ✅ Role-Based Access (Admin, User roles)
- ✅ Profile Management with user preferences
- ✅ Administrative user control and blocking

### 📊 Inventory Management
- ✅ Create inventories with custom categorization
- ✅ Public/Private visibility control
- ✅ **Custom Field Configuration** (up to 15 different field types):
  - 📝 Text fields (3)
  - 📄 Multiline text fields (3)
  - 🔢 Numeric fields with validation (3)
  - 📎 Document/Image fields (3)
  - ✅ Boolean fields (3)
- ✅ Configurable Custom ID generation

### 📦 Item Management
- ✅ Full CRUD operations for items
- ✅ Custom field values based on inventory configuration
- ✅ Image upload with Cloudinary integration
- ✅ Social features (item likes)
- ✅ Advanced search and filtering

### 🤝 Collaboration Features
- ✅ Granular access control for inventories
- ✅ Multiple permission levels (View, Edit, Manage)
- ✅ Real-time comments with SignalR
- ✅ Activity tracking and monitoring

### 🔧 Advanced Features
- ✅ RESTful API with token authentication
- ✅ Data aggregation and statistical analysis
- ✅ Data export capabilities
- ✅ Integrated support ticket system with cloud storage

### ⚡ Admin Features
- ✅ Comprehensive user management
- ✅ System monitoring and analytics
- ✅ Configuration management

## 📁 Project Structure

```
InventoryMgmt.MVC/
├── 🎯 InventoryMgmt.MVC/          # Presentation Layer
│   ├── Controllers/                # MVC Controllers
│   ├── Views/                      # Razor Views
│   ├── Models/                     # ViewModels
│   ├── Hubs/                       # SignalR Hubs
│   ├── Attributes/                 # Custom Attributes
│   ├── Helpers/                    # Helper Classes
│   └── wwwroot/                    # Static Files (CSS, JS, Images)
├── 🏢 InventoryMgmt.BLL/          # Business Logic Layer
│   ├── Services/                   # Business Services
│   ├── Interfaces/                 # Service Contracts
│   ├── DTOs/                       # Data Transfer Objects
│   └── Profiles/                   # AutoMapper Profiles
└── 🗄️ InventoryMgmt.DAL/          # Data Access Layer
    ├── Repos/                      # Repository Implementations
    ├── Interfaces/                 # Repository Contracts
    ├── EF/TableModels/             # Entity Models
    ├── Data/                       # DbContext
    └── Migrations/                 # EF Migrations
```

## 🎯 Design Principles

### SOLID Principles Implementation

#### ✅ **Single Responsibility Principle (SRP)**
- Each service class has a single, well-defined responsibility
- Controllers focus only on HTTP request/response handling
- Repositories handle only data access operations

#### ✅ **Open/Closed Principle (OCP)**
- Services depend on interfaces, allowing easy extension
- Custom field processors can be extended without modifying existing code
- New authentication providers can be added without changing core logic

#### ✅ **Liskov Substitution Principle (LSP)**
- All implementations properly substitute their interfaces
- Repository implementations are interchangeable
- Service implementations follow their contracts

#### ✅ **Interface Segregation Principle (ISP)**
- Small, focused interfaces (`IInventoryService`, `IUserService`)
- Clients depend only on methods they actually use
- Specialized interfaces for different concerns

#### ✅ **Dependency Inversion Principle (DIP)**
- High-level modules don't depend on low-level modules
- Both depend on abstractions (interfaces)
- Dependency injection container manages dependencies

### Additional Design Principles

- **🎯 Separation of Concerns**: Clear layer boundaries with specific responsibilities
- **🔄 Don't Repeat Yourself (DRY)**: Common functionality in base classes and shared components
- **📋 Convention over Configuration**: Following ASP.NET Core and EF conventions

## 🔒 Security Features

### 🔐 Authentication
- **Multi-factor Authentication**: Support for external providers
- **Secure Password Storage**: Industry-standard password hashing
- **Session Management**: Secure cookie configuration with sliding expiration

### 🛡️ Authorization
- **Role-based Security**: Admin and User roles
- **Resource-based Authorization**: Inventory-specific permissions
- **API Security**: JWT token-based authentication for API access

### 🔒 Data Protection
- **SQL Injection Prevention**: Entity Framework parameterized queries
- **XSS Protection**: Razor view engine automatic encoding
- **CSRF Protection**: Anti-forgery tokens
- **HTTPS Enforcement**: Secure communication

### 📋 Privacy & Compliance
- **Data Minimization**: Only necessary data collection
- **User Control**: Users control their data sharing
- **Audit Trail**: Activity logging for compliance

## 🔗 Integration Capabilities

### 🏢 Salesforce Integration
- **Account Creation**: Automatic Salesforce account creation
- **Contact Management**: Sync user information
- **Authentication**: OAuth 2.0 integration

### ☁️ Cloud Storage Integration
- **Cloudinary**: Image storage and transformation
- **Dropbox**: Document storage for support tickets
- **OneDrive**: Alternative document storage option

### 🔌 API Access
- **RESTful Endpoints**: Full CRUD operations via API
- **Token Authentication**: Secure API access
- **Rate Limiting**: Configurable API usage limits

## 🗄️ Database Design

### Core Entities

#### 👥 **Users**
- Authentication and profile information
- Role-based permissions
- User preferences and settings

#### 📦 **Inventories**
- Collection metadata and configuration
- Custom field definitions
- Visibility and access control

#### 📝 **Items**
- Inventory contents with custom field values
- Versioning and audit trail
- Social interaction data (likes, comments)

#### 🔐 **Access Control**
- Granular permission system
- User-inventory relationships
- Permission inheritance

### Database Features
- **⚡ Concurrency Control**: Timestamp-based optimistic locking
- **📊 Indexing**: Strategic indexes for performance
- **🔒 Constraints**: Data integrity enforcement
- **🔄 Migrations**: Version-controlled schema changes

## 🚀 Getting Started

### Prerequisites
- .NET 8.0 SDK
- SQL Server (LocalDB or full instance)
- Visual Studio 2022 or VS Code
- Git

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/tanvirshaad/InventoryMgmt.git
   cd InventoryMgmt
   ```

2. **Restore packages**
   ```bash
   dotnet restore
   ```

3. **Update database connection string**
   ```json
   // appsettings.json
   {
     "ConnectionStrings": {
       "InventoryMgmt": "Server=(localdb)\\mssqllocaldb;Database=InventoryMgmtDB;Trusted_Connection=true;"
     }
   }
   ```

4. **Apply database migrations**
   ```bash
   dotnet ef database update --project InventoryMgmt.DAL --startup-project InventoryMgmt.MVC
   ```

5. **Run the application**
   ```bash
   dotnet run --project InventoryMgmt.MVC
   ```

6. **Access the application**
   - Navigate to `https://localhost:5001`
   - Default admin credentials: `admin@example.com` / `Admin123!`

## ⚙️ Configuration

### Environment Variables (.env file)

```bash
# JWT Configuration
JWT_KEY=YourSuperSecretKeyHere
JWT_ISSUER=InventoryMgmt
JWT_AUDIENCE=InventoryMgmtUsers
JWT_EXPIRY_DAYS=7

# Cloudinary Configuration
CLOUDINARY_CLOUD_NAME=your_cloud_name
CLOUDINARY_API_KEY=your_api_key
CLOUDINARY_API_SECRET=your_api_secret

# Google OAuth
GOOGLE_CLIENT_ID=your_google_client_id
GOOGLE_CLIENT_SECRET=your_google_client_secret

# Salesforce Integration
SALESFORCE_CLIENT_ID=your_salesforce_client_id
SALESFORCE_CLIENT_SECRET=your_salesforce_client_secret
SALESFORCE_USERNAME=your_salesforce_username
SALESFORCE_PASSWORD=your_salesforce_password

# Support Ticket Configuration
SUPPORT_TICKET_ADMIN_EMAILS=admin@example.com
DROPBOX_ACCESS_TOKEN=your_dropbox_token
```

## 📚 API Documentation

### Authentication
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "password123"
}
```

### Inventory Operations
```http
# Get all inventories
GET /api/inventories
Authorization: Bearer {jwt_token}

# Create inventory
POST /api/inventories
Authorization: Bearer {jwt_token}
Content-Type: application/json

{
  "title": "My Inventory",
  "description": "Description here",
  "categoryId": 1,
  "isPublic": true
}
```

### Item Operations
```http
# Get inventory items
GET /api/inventories/{id}/items
Authorization: Bearer {jwt_token}

# Create item
POST /api/inventories/{id}/items
Authorization: Bearer {jwt_token}
Content-Type: application/json
```

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 👨‍💻 Author

**Tanvir Shaad**
- GitHub: [@tanvirshaad](https://github.com/tanvirshaad)
- Email: tanvirshaad@gmail.com

## 🙏 Acknowledgments

- ASP.NET Core team for the excellent framework
- Entity Framework team for the robust ORM
- Bootstrap team for the responsive UI framework
- SignalR team for real-time communication capabilities

---

⭐ **Star this repository if you find it helpful!**