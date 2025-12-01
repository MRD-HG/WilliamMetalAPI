# William Metal API - Backend Documentation

## Overview
.NET 8 Web API backend for William Metal Management System with PostgreSQL database, JWT authentication, and comprehensive business logic for managing metal products, inventory, sales, and purchases.

## Technology Stack
- **Framework**: .NET 8 Web API
- **Database**: PostgreSQL with Entity Framework Core 8
- **Authentication**: JWT Bearer Tokens
- **Documentation**: Swagger/OpenAPI
- **Auto Mapping**: AutoMapper
- **Password Hashing**: BCrypt
- **CORS**: Enabled for cross-origin requests

## Project Structure

```
backend/
├── Controllers/          # API Controllers
│   ├── AuthController.cs
│   ├── ProductsController.cs
│   ├── InventoryController.cs
│   ├── SalesController.cs
│   ├── PurchasesController.cs
│   └── DashboardController.cs
├── Models/              # Entity Models
│   ├── Product.cs
│   ├── Inventory.cs
│   ├── Sale.cs
│   ├── Purchase.cs
│   ├── User.cs
│   └── Settings.cs
├── DTOs/                # Data Transfer Objects
│   ├── ProductDTO.cs
│   ├── InventoryDTO.cs
│   ├── SaleDTO.cs
│   ├── PurchaseDTO.cs
│   └── DashboardDTO.cs
├── Services/            # Business Logic Services
│   ├── IAuthService.cs
│   ├── AuthService.cs
│   ├── IProductService.cs
│   ├── ProductService.cs
│   ├── IInventoryService.cs
│   ├── InventoryService.cs
│   ├── ISaleService.cs
│   ├── SaleService.cs
│   ├── IPurchaseService.cs
│   ├── PurchaseService.cs
│   ├── IDashboardService.cs
│   └── DashboardService.cs
├── Data/                # Database Context
│   ├── WilliamMetalContext.cs
│   └── DbInitializer.cs
├── Mapping/             # AutoMapper Profiles
│   └── MappingProfile.cs
├── Program.cs           # Application Entry Point
├── WilliamMetalAPI.csproj
├── appsettings.json     # Configuration
└── README.md
```

## Database Schema

### Core Tables
- **Users**: Authentication and user management
- **Products**: Product catalog with categories and variants
- **ProductVariants**: Product specifications, prices, and stock
- **InventoryMovements**: Stock movement history
- **Customers**: Customer information
- **Sales**: Sales transactions with items
- **SaleItems**: Individual items in sales
- **Suppliers**: Supplier information
- **Purchases**: Purchase orders with items
- **PurchaseItems**: Individual items in purchases
- **Settings**: Company, inventory, and notification settings

## API Endpoints

### Authentication
- `POST /api/auth/login` - User login
- `GET /api/auth/me` - Get current user
- `POST /api/auth/change-password` - Change password
- `POST /api/auth/validate-token` - Validate JWT token

### Products
- `GET /api/products` - Get all products (with filtering)
- `GET /api/products/{id}` - Get product by ID
- `POST /api/products` - Create new product
- `PUT /api/products/{id}` - Update product
- `DELETE /api/products/{id}` - Delete product
- `GET /api/products/categories` - Get product categories
- `GET /api/products/search` - Search products
- `POST /api/products/{id}/variants` - Add product variant
- `PUT /api/products/{id}/variants/{variantId}` - Update variant
- `DELETE /api/products/{id}/variants/{variantId}` - Delete variant

### Inventory
- `GET /api/inventory/stats` - Get inventory statistics
- `GET /api/inventory/movements` - Get inventory movements
- `GET /api/inventory/alerts` - Get stock alerts
- `POST /api/inventory/update-stock` - Update stock levels
- `POST /api/inventory/adjust-stock` - Adjust stock manually

### Sales
- `GET /api/sales` - Get all sales (with filtering)
- `GET /api/sales/{id}` - Get sale by ID
- `POST /api/sales` - Create new sale
- `PUT /api/sales/{id}/status` - Update sale status
- `DELETE /api/sales/{id}` - Delete sale
- `GET /api/sales/customers` - Get customers
- `POST /api/sales/customers` - Create customer
- `GET /api/sales/invoice-number` - Generate invoice number

### Purchases
- `GET /api/purchases` - Get all purchases (with filtering)
- `GET /api/purchases/{id}` - Get purchase by ID
- `POST /api/purchases` - Create new purchase
- `PUT /api/purchases/{id}/status` - Update purchase status
- `DELETE /api/purchases/{id}` - Delete purchase
- `GET /api/purchases/suppliers` - Get suppliers
- `POST /api/purchases/suppliers` - Create supplier
- `GET /api/purchases/purchase-number` - Generate purchase number

### Dashboard
- `GET /api/dashboard/stats` - Get dashboard statistics
- `GET /api/dashboard/data` - Get complete dashboard data
- `GET /api/dashboard/sales-chart` - Get sales chart data
- `GET /api/dashboard/stock-alerts` - Get stock alerts
- `GET /api/dashboard/top-products` - Get top selling products

## Setup Instructions

### Prerequisites
- .NET 8 SDK
- PostgreSQL 12+
- Visual Studio 2022 or VS Code

### Database Setup
1. Install PostgreSQL and create a database named `williammetal`
2. Update connection string in `appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Database=williammetal;Username=postgres;Password=yourpassword;Port=5432"
}
```

### JWT Configuration
Update JWT settings in `appsettings.json`:
```json
"JwtSettings": {
  "SecretKey": "your-very-secure-secret-key-here-should-be-at-least-32-characters-long",
  "Issuer": "WilliamMetalAPI",
  "Audience": "WilliamMetalUsers",
  "ExpireMinutes": 1440
}
```

### Running the Application

1. **Restore Dependencies**
```bash
dotnet restore
```

2. **Apply Database Migrations**
```bash
dotnet ef database update
```

3. **Run the Application**
```bash
dotnet run
```

The API will be available at `https://localhost:5001` with Swagger documentation at `/swagger`.

### Default Login
- **Username**: admin
- **Password**: admin123

## Configuration

### CORS Policy
The API is configured to allow all origins for development. For production, update the CORS policy in `Program.cs`.

### Database Migrations
Use Entity Framework Core commands to manage database schema:
```bash
# Create migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update

# Remove migration
dotnet ef migrations remove
```

## Security Features

### Authentication
- JWT Bearer token authentication
- Password hashing with BCrypt
- Role-based authorization (Admin, Manager, Employee)

### Authorization
- Admin: Full access to all endpoints
- Manager: Access to most endpoints except user management
- Employee: Limited access to sales and inventory operations

### Data Protection
- Entity Framework Core for SQL injection prevention
- Input validation and sanitization
- Secure password storage with BCrypt hashing

## Error Handling

### Standard Response Format
All API responses follow a consistent format:
```json
{
  "success": true/false,
  "data": { ... },
  "message": "Optional message"
}
```

### Error Codes
- `200`: Success
- `400`: Bad Request
- `401`: Unauthorized
- `403`: Forbidden
- `404`: Not Found
- `500`: Internal Server Error

## Performance Considerations

### Database Optimization
- Proper indexing on frequently queried columns
- Entity Framework Core query optimization
- Pagination for large datasets

### Caching
- In-memory caching can be added for frequently accessed data
- Response caching for static content

### API Optimization
- Asynchronous operations throughout
- Efficient data transfer with DTOs
- Minimal database queries with proper includes

## Deployment

### Production Environment
1. Update connection strings for production database
2. Configure proper CORS origins
3. Set up SSL certificates
4. Configure logging and monitoring
5. Set up backup and disaster recovery

### Docker Support
Create a Dockerfile for containerized deployment:
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY . .
ENTRYPOINT ["dotnet", "WilliamMetalAPI.dll"]
```

## Monitoring and Logging

### Built-in Logging
- ASP.NET Core built-in logging
- Structured logging with Serilog (can be added)

### Health Checks
- Database connectivity health checks
- API endpoint health checks

### Metrics
- Request/response metrics
- Database performance metrics
- Business metrics (sales, inventory levels)

## Maintenance

### Regular Tasks
- Database backup and maintenance
- Log file management
- Security updates
- Performance monitoring

### Updates
- Keep .NET runtime updated
- Update NuGet packages regularly
- Monitor for security advisories

## Support

For technical support or questions about the API, please refer to the documentation or contact the development team.