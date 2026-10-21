using System.Data;
using System.Text;
using Microsoft.Data.SqlClient;

namespace DatabaseAiAssistant.Services;

public class SalesDatabaseService
{
    private readonly string _connectionString;

    public SalesDatabaseService()
    {
        _connectionString =
            Environment.GetEnvironmentVariable(
                "DB_CONNECTION_STRING")
            ?? @"Server=(localdb)\MSSQLLocalDB;
                 Database=AgentSalesDB;
                 Integrated Security=True;
                 Encrypt=True;
                 TrustServerCertificate=True;";
    }

    public async Task<string> GetLowStockAsync(
        int threshold)
    {
        if (threshold < 0)
            throw new ArgumentException(
                "Stock threshold cannot be negative.");

        const string sql = """
            SELECT ProductName, StockQuantity, UnitPrice
            FROM dbo.Products
            WHERE StockQuantity <= @Threshold
            ORDER BY StockQuantity, ProductName;
            """;

        var result = new StringBuilder();

        await using var connection =
            new SqlConnection(_connectionString);

        await using var command =
            new SqlCommand(sql, connection);

        command.Parameters.Add(
            "@Threshold", SqlDbType.Int).Value = threshold;

        await connection.OpenAsync();

        await using var reader =
            await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            result.AppendLine(
                $"{reader.GetString(0)}: " +
                $"Stock {reader.GetInt32(1)}, " +
                $"Price RM{reader.GetDecimal(2):F2}");
        }

        return result.Length == 0
            ? "No low-stock products found."
            : result.ToString();
    }

    public async Task<string> GetCustomerBalanceAsync(
        string customerName)
    {
        if (string.IsNullOrWhiteSpace(customerName))
            throw new ArgumentException(
                "Customer name is required.");

        const string sql = """
            SELECT CustomerName, OutstandingBalance
            FROM dbo.Customers
            WHERE CustomerName = @CustomerName;
            """;

        await using var connection =
            new SqlConnection(_connectionString);

        await using var command =
            new SqlCommand(sql, connection);

        command.Parameters.Add(
            "@CustomerName", SqlDbType.NVarChar, 100)
            .Value = customerName.Trim();

        await connection.OpenAsync();

        await using var reader =
            await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return
                $"Customer: {reader.GetString(0)}\n" +
                $"Outstanding Balance: " +
                $"RM{reader.GetDecimal(1):F2}";
        }

        return "Customer not found.";
    }

    public async Task<string> GetSalesSummaryAsync(
        DateTime startDate,
        DateTime endDateExclusive)
    {
        if (endDateExclusive <= startDate)
            throw new ArgumentException(
                "End date must be after start date.");

        const string sql = """
            SELECT
                COALESCE(SUM(TotalAmount), 0) AS TotalSales,
                COUNT(*) AS NumberOfSales,
                COALESCE(AVG(TotalAmount), 0) AS AverageSale
            FROM dbo.Sales
            WHERE SaleDate >= @StartDate
              AND SaleDate < @EndDate;
            """;

        await using var connection =
            new SqlConnection(_connectionString);

        await using var command =
            new SqlCommand(sql, connection);

        command.Parameters.Add(
            "@StartDate", SqlDbType.DateTime2)
            .Value = startDate;

        command.Parameters.Add(
            "@EndDate", SqlDbType.DateTime2)
            .Value = endDateExclusive;

        await connection.OpenAsync();

        await using var reader =
            await command.ExecuteReaderAsync();

        await reader.ReadAsync();

        decimal total = reader.GetDecimal(0);
        int count = reader.GetInt32(1);
        decimal average = reader.GetDecimal(2);

        return $"""
            Sales Period: {startDate:yyyy-MM-dd} to
            {endDateExclusive.AddDays(-1):yyyy-MM-dd}
            Total Sales: RM{total:F2}
            Number of Sales: {count}
            Average Sale: RM{average:F2}
            """;
    }

    public async Task<string> GetTopProductsAsync(
        DateTime startDate,
        DateTime endDateExclusive,
        int topCount = 5)
    {
        if (topCount < 1 || topCount > 20)
            throw new ArgumentException(
                "Top count must be between 1 and 20.");

        if (endDateExclusive <= startDate)
            throw new ArgumentException(
                "End date must be after start date.");

        const string sql = """
            SELECT TOP (@TopCount)
                p.ProductName,
                SUM(si.Quantity) AS QuantitySold,
                SUM(si.Quantity * si.UnitPrice) AS SalesAmount
            FROM dbo.SaleItems si
            INNER JOIN dbo.Products p
                ON si.ProductId = p.ProductId
            INNER JOIN dbo.Sales s
                ON si.SaleId = s.SaleId
            WHERE s.SaleDate >= @StartDate
              AND s.SaleDate < @EndDate
            GROUP BY p.ProductName
            ORDER BY QuantitySold DESC, p.ProductName;
            """;

        var result = new StringBuilder();

        await using var connection =
            new SqlConnection(_connectionString);

        await using var command =
            new SqlCommand(sql, connection);

        command.Parameters.Add(
            "@TopCount", SqlDbType.Int).Value = topCount;

        command.Parameters.Add(
            "@StartDate", SqlDbType.DateTime2)
            .Value = startDate;

        command.Parameters.Add(
            "@EndDate", SqlDbType.DateTime2)
            .Value = endDateExclusive;

        await connection.OpenAsync();

        await using var reader =
            await command.ExecuteReaderAsync();

        int rank = 1;

        while (await reader.ReadAsync())
        {
            result.AppendLine(
                $"{rank}. {reader.GetString(0)} - " +
                $"Quantity: {reader.GetInt32(1)}, " +
                $"Sales: RM{reader.GetDecimal(2):F2}");

            rank++;
        }

        return result.Length == 0
            ? "No sales records found for this period."
            : result.ToString();
    }
}

