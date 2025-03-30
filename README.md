# .NET 9 Solution

## Prerequisites
To run this solution, ensure you have the following installed:

- [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
- [.NET 9 Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
- [.NET 9 Hosting Bundle](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)

## Database Support
This solution supports the following databases:
- **Microsoft SQL Server**
- **PostgreSQL**
- **MySQL**

## Configuration
Update the `appsettings.json` file with the appropriate connection string for your database. Example:

### **Microsoft SQL Server**
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=YOUR_DATABASE_NAME;User ID=YOUR_USERNAME;Password=YOUR_PASSWORD;TrustServerCertificate=True;"
}
```

### **PostgreSQL**
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=YOUR_HOST;Port=5432;Database=YOUR_DATABASE;Username=YOUR_USERNAME;Password=YOUR_PASSWORD;"
}
```

### **MySQL**
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=YOUR_DATABASE_NAME;User ID=YOUR_USERNAME;Password=YOUR_PASSWORD;"
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

## Deployment
For deployment instructions, ensure you install the **.NET 9 Hosting Bundle** on the target server and configure the environment variables accordingly.

---
For more details, refer to the [official .NET 9 documentation](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-9).
