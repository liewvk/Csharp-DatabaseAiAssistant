# Csharp Database AI Assistant

A C# console application that connects to a SQL Server database and uses Ollama to generate concise business summaries from query results.

This project demonstrates a simple AI-assisted data analysis workflow:

- Query sales data from SQL Server
- Display business results in the terminal
- Ask Ollama to summarize the retrieved data
- Keep the AI response grounded in the returned database output

## Features

- Low-stock product lookup
- Customer balance lookup
- Sales summary over a date range
- Top-selling products by quantity and revenue
- AI summary generation via Ollama using the `qwen3.5:9b` model

## Project Structure

- `Program.cs` - console menu and orchestration logic
- `Services/SalesDatabaseService.cs` - database access and SQL queries
- `Services/OllamaSummaryService.cs` - Ollama API integration
- `DatabaseAiAssistant.csproj` - .NET project configuration

## Tech Stack

- C#
- .NET 9 (project target may vary by environment)
- Microsoft.Data.SqlClient
- SQL Server / LocalDB
- Ollama local API

## Prerequisites

Before running the app, make sure you have:

- .NET SDK installed
- SQL Server or LocalDB available
- A database named `AgentSalesDB` with the required tables
- Ollama installed and running locally on `http://localhost:11434`
- The model `qwen3.5:9b` pulled in Ollama

## Database Requirements

The application expects SQL Server tables such as:

- `dbo.Products`
- `dbo.Customers`
- `dbo.Sales`
- `dbo.SaleItems`

The code reads columns including:

- `ProductName`
- `StockQuantity`
- `UnitPrice`
- `CustomerName`
- `OutstandingBalance`
- `SaleDate`
- `TotalAmount`
- `Quantity`

## Configuration

The app reads the database connection string from the `DB_CONNECTION_STRING` environment variable.

If the environment variable is not set, it falls back to a LocalDB connection string:

```text
Server=(localdb)\MSSQLLocalDB;Database=AgentSalesDB;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;
```

You can set it in Windows PowerShell like this:

```powershell
$env:DB_CONNECTION_STRING = "Server=(localdb)\\MSSQLLocalDB;Database=AgentSalesDB;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;"
```

## Running the Application

From the repository root:

```bash
dotnet restore
dotnet build
dotnet run
```

You will see a menu with options such as:

1. Display Low-Stock Products
2. Get Customer Balance
3. Get Sales Summary
4. Get Top-Selling Products
0. Exit

After a database query, the app asks whether to generate an AI summary using Ollama.

## Example Workflow

1. Run the app.
2. Select a reporting option.
3. View the raw database results.
4. Choose `Y` to generate an AI summary.
5. Review the concise explanation provided by Ollama.

## Notes

- The AI layer is intentionally constrained to summarize the data that was retrieved.
- It does not invent figures or execute database changes.
- The app expects the database schema to match the queries in `SalesDatabaseService.cs`.

## License

This project is provided as-is for educational and demonstration purposes.
