# .NET 10 Solution

## Prerequisites
To run this solution, ensure you have the following installed:

- [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
- [.NET 10 Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
- [.NET 10 Hosting Bundle](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)

## Database Support
This solution supports the following databases:
- **Microsoft SQL Server**
- **PostgreSQL**
- **MySQL**
- **SqLite**

## Configuration
Update the `appsettings.json` file with the appropriate connection string for your database. Example:

### **Microsoft SQL Server**
```json
"ConnectionStrings": {
  "Engine" : "MsSql",
  "Db": "Server=YOUR_SERVER_NAME;Database=YOUR_DATABASE_NAME;User ID=YOUR_USERNAME;Password=YOUR_PASSWORD;TrustServerCertificate=True;"
}
```

### **PostgreSQL**
```json
"ConnectionStrings": {
  "Engine" : "PgSql",
  "Db": "Host=YOUR_HOST;Port=5432;Database=YOUR_DATABASE;Username=YOUR_USERNAME;Password=YOUR_PASSWORD;"
}
```

### **MySQL**
```json
"ConnectionStrings": {
  "Engine" : "MySql",
  "Db": "Server=YOUR_SERVER_NAME;Database=YOUR_DATABASE_NAME;User ID=YOUR_USERNAME;Password=YOUR_PASSWORD;"
}
```

### **SqLite**
```json
"ConnectionStrings": {
  "Engine" : "SqLite",
  "Db": "Data Source={path_to_file};Version={version_number};"
}
```

## Running the Application
1. Restore dependencies:
   ```sh
   dotnet restore
   ```
2. Build the project:
   ```sh
   dotnet build
   ```
3. Run the application:
   ```sh
   dotnet run
   ```
## Migrations
For migrations, add a `migration-X.sql` script under `Persistence/Migrations/{Engine}` for each database engine you intend to support. The configured engine is selected from `ConnectionStrings:Engine`, and the matching migration folder is used when the solution runs migrations.

## Data Access
The persistence layer separates read and write entry points:

- `IReadOnlyRepository` is intended for queries and does not expose the write-capable SqlKata `QueryFactory`.
- `IWritableRepository` is intended for commands and migrations.
- `ICommandRunner` executes raw commands and engine-specific bulk insert operations.
- Repositories and command runners can use the configured connection through DI or be created manually with a connection string and database engine when a service needs a separate database target.

## Shared Helpers
The `Common` project includes reusable helpers:

- `HttpCall` for JSON, form and string HTTP calls.
- `FileUploader` for multipart file uploads.
- `TaskRunner` for bounded-concurrency asynchronous work.

## Cqrs
Command Query Responsibility Segregation (CQRS) is a design pattern that segregates read and write operations
for a data store into separate data models. This approach allows each model to be optimized independently
and can improve the performance, scalability, and security of an application.

---
For more details, refer to [Cqrs on microsoft](https://learn.microsoft.com/es-mx/azure/architecture/patterns/cqrs)


## Deployment
For deployment instructions, ensure you install the **.NET 10 Hosting Bundle** on the target server and configure the environment variables accordingly.

---
For more details, refer to the [official .NET 10 documentation](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-10/overview).
