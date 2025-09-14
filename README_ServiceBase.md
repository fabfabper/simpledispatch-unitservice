# SimpleDispatch Unit Service

A microservice built using the SimpleDispatch.ServiceBase framework for handling unit-related operations with RabbitMQ messaging and REST API capabilities.

## Overview

This service has been refactored to use the SimpleDispatch.ServiceBase package, which provides:

- **RabbitMQ Integration**: Built-in message consumption and publishing
- **REST API**: Full CRUD operations for units
- **Database Integration**: PostgreSQL with Entity Framework Core
- **Message Handling**: Custom unit message processing
- **Health Checks**: Built-in health check endpoints
- **Logging**: Structured logging throughout the application

## Architecture

### Main Components

1. **UnitService** (`UnitService.cs`) - Main service class inheriting from `BaseService`
2. **UnitMessageHandler** (`UnitMessageHandler.cs`) - Handles RabbitMQ messages
3. **UnitsController** (`Controllers/UnitsController.cs`) - REST API endpoints with transaction support
4. **DatabaseRepository** (`Repositories/DatabaseRepository.cs`) - Data access layer inheriting from `BaseRepository<Unit, string, BaseDbContext>`
5. **IDatabaseRepository** (`Repositories/IDatabaseRepository.cs`) - Repository interface extending `IRepository<Unit, string>`

### Repository Pattern

The service now uses the ServiceBase repository pattern:

- **Base Repository**: `DatabaseRepository` inherits from `BaseRepository<Unit, string, BaseDbContext>`
- **Base DbContext**: Uses the framework's `BaseDbContext` directly (no custom DbContext needed)
- **Interface**: `IDatabaseRepository` extends `IRepository<Unit, string>` from ServiceBase
- **Built-in Methods**: Includes standard CRUD operations (GetAllAsync, GetByIdAsync, AddAsync, UpdateAsync, DeleteAsync, SaveChangesAsync)
- **Custom Methods**: Additional methods like `GetUnitsByStatusAsync()` and `GetActiveUnitsAsync()`
- **Transaction Support**: Full Unit of Work pattern with `IUnitOfWork` for transaction management

## Configuration

The service is configured via `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=simpledispatch;Username=postgres;Password=password"
  },
  "Database": {
    "MaxRetryCount": 3,
    "MaxRetryDelay": 30,
    "EnableSensitiveDataLogging": false,
    "EnableDetailedErrors": false,
    "CommandTimeout": 30
  },
  "RabbitMq": {
    "HostName": "localhost",
    "Port": 5672,
    "UserName": "guest",
    "Password": "guest",
    "VirtualHost": "/",
    "QueueName": "units",
    "ExchangeName": "simpledispatch.exchange",
    "ExchangeType": "direct",
    "Durable": true,
    "AutoAck": false,
    "PrefetchCount": 1
  }
}
```

## API Endpoints

### Units Controller

- `GET /api/units` - Get all units
- `GET /api/units/{id}` - Get unit by ID
- `POST /api/units` - Create new unit
- `PUT /api/units/{id}` - Update existing unit
- `DELETE /api/units/{id}` - Delete unit

### Built-in Endpoints

- `GET /health` - Health check endpoint
- `GET /api/messaging/health` - Messaging service health check
- `POST /api/messaging/publish` - Publish a message to RabbitMQ

## Message Types

The service handles the following message types:

- `UnitCreated` - When a new unit is created
- `UnitUpdated` - When an existing unit is updated
- `UnitStatusChanged` - When a unit's status changes
- `UnitDeleted` - When a unit is deleted

## Running the Service

1. Ensure PostgreSQL and RabbitMQ are running
2. Update connection strings in `appsettings.json`
3. Run database migrations if needed:
   ```bash
   dotnet ef database update
   ```
4. Start the service:
   ```bash
   dotnet run
   ```

## Dependencies

- .NET 9.0
- SimpleDispatch.ServiceBase 1.2.0
- SimpleDispatch.SharedModels 1.2.0
- Entity Framework Core
- Npgsql.EntityFrameworkCore.PostgreSQL

## Key Changes from Original Implementation

1. **Removed manual RabbitMQ setup** - Now handled by ServiceBase
2. **Simplified Program.cs** - Uses ServiceBase initialization pattern
3. **Added REST API** - Full CRUD operations with automatic message publishing
4. **Enhanced message handling** - Structured message processing with JSON parsing
5. **Improved configuration** - Standard ServiceBase configuration format
6. **Better error handling** - Consistent API responses and logging
7. **Repository Pattern** - Now uses ServiceBase `BaseRepository<TEntity, TKey, TContext>` pattern
8. **Simplified DbContext** - Uses framework's `BaseDbContext` directly, no custom DbContext needed
9. **Transaction Management** - Integrated `IUnitOfWork` for proper transaction handling
10. **Type Safety** - Unit entity uses string IDs as per SharedModels specification

## Benefits of ServiceBase Integration

- **Reduced Boilerplate**: The ServiceBase handles infrastructure concerns
- **Standardized Patterns**: Follows established SimpleDispatch framework conventions
- **Built-in Features**: Health checks, transaction management, and message handling
- **Type Safety**: Proper generic repository pattern with compile-time type checking
- **Maintainability**: Cleaner separation of concerns and consistent error handling
- **Scalability**: Built-in monitoring, logging, and performance optimizations
