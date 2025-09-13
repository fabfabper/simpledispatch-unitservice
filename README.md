# SimpleDispatch Unit Service

A .NET 9 background service that consumes messages from RabbitMQ and interacts with a PostgreSQL database to manage units.

## Features

- **RabbitMQ Integration**: Listens to the "units" queue and logs received messages
- **PostgreSQL Database**: Repository pattern for managing unit data
- **Background Service**: Runs continuously as a hosted service
- **Dependency Injection**: Properly configured DI container
- **Logging**: Comprehensive logging throughout the application

## Project Structure

```
├── Data/
│   └── ApplicationDbContext.cs    # Entity Framework DbContext
├── Models/
│   └── Unit.cs                   # Unit entity model
├── Repositories/
│   ├── IDatabaseRepository.cs    # Repository interface
│   └── DatabaseRepository.cs     # Repository implementation
├── Program.cs                    # Application entry point
├── Worker.cs                     # Background service worker
├── RabbitMQConfiguration.cs      # RabbitMQ configuration model
└── database-setup.sql           # Database setup script
```

## Setup Instructions

### Prerequisites

- .NET 9.0 SDK
- PostgreSQL database server
- RabbitMQ server

### Database Setup

1. Create a PostgreSQL database:

   ```sql
   CREATE DATABASE simpledispatch_dev;
   ```

2. Run the setup script:
   ```bash
   psql -d simpledispatch_dev -f database-setup.sql
   ```

### Configuration

Update the connection strings in `appsettings.json` and `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=simpledispatch_dev;Username=your_username;Password=your_password"
  },
  "RabbitMQ": {
    "HostName": "localhost",
    "Port": 5672,
    "UserName": "guest",
    "Password": "guest",
    "QueueName": "units"
  }
}
```

### Running the Service

```bash
dotnet restore
dotnet build
dotnet run
```

## Database Repository

The `IDatabaseRepository` interface provides the following methods:

- `GetAllUnitsAsync()` - Get all units
- `GetUnitByIdAsync(int id)` - Get unit by ID
- `GetUnitsByStatusAsync(string status)` - Get units by status
- `GetActiveUnitsAsync()` - Get only active units
- `CreateUnitAsync(Unit unit)` - Create a new unit
- `UpdateUnitAsync(Unit unit)` - Update an existing unit
- `DeleteUnitAsync(int id)` - Delete a unit
- `UnitExistsAsync(int id)` - Check if unit exists

## Unit Model

The `Unit` entity includes the following properties:

- `Id` - Primary key
- `Name` - Unit name (required)
- `Type` - Unit type (e.g., Vehicle, Equipment)
- `Status` - Current status (e.g., Available, In Transit, Maintenance)
- `Location` - Current location
- `CreatedAt` - Creation timestamp
- `UpdatedAt` - Last update timestamp
- `IsActive` - Active status flag

## Usage Example

When a message is received from the RabbitMQ "units" queue, the service will:

1. Log the received message
2. Query the database for active units
3. Log information about the retrieved units
4. Acknowledge the message

This demonstrates how the RabbitMQ consumer can trigger database operations using the repository pattern.
