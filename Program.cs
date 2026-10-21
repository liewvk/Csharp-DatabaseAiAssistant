using DatabaseAiAssistant.Services;

Console.Title = "C# Database AI Assistant";

var database = new SalesDatabaseService();
using var aiService = new OllamaSummaryService();

Console.WriteLine("==================================");
Console.WriteLine("       DATABASE AI ASSISTANT");
Console.WriteLine("       Visual Studio 2026");
Console.WriteLine("       SQL Server + Ollama");
Console.WriteLine("==================================");

while (true)
{
    Console.WriteLine();
    Console.WriteLine("1. Display Low-Stock Products");
    Console.WriteLine("2. Get Customer Balance");
    Console.WriteLine("3. Get Sales Summary");
    Console.WriteLine("4. Get Top-Selling Products");
    Console.WriteLine("0. Exit");
    Console.WriteLine();

    Console.Write("Select option: ");
    string? choice = Console.ReadLine();

    if (choice == "0")
        break;

    try
    {
        string data;

        switch (choice)
        {
            case "1":
                Console.Write("Stock threshold: ");

                if (!int.TryParse(
                    Console.ReadLine(), out int threshold))
                {
                    Console.WriteLine("Invalid threshold.");
                    continue;
                }

                data = await database.GetLowStockAsync(
                    threshold);

                break;

            case "2":
                Console.Write("Customer name: ");

                string customerName =
                    Console.ReadLine() ?? "";

                data = await database
                    .GetCustomerBalanceAsync(customerName);

                break;

            case "3":
                data = await database.GetSalesSummaryAsync(
                    new DateTime(2026, 10, 1),
                    new DateTime(2026, 11, 1));

                break;

            case "4":
                data = await database.GetTopProductsAsync(
                    new DateTime(2026, 10, 1),
                    new DateTime(2026, 11, 1),
                    topCount: 5);

                break;

            default:
                Console.WriteLine("Invalid selection.");
                continue;
        }

        Console.WriteLine();
        Console.WriteLine("Database Results:");
        Console.WriteLine(data);

        Console.WriteLine();
        Console.Write("Generate AI summary? (Y/N): ");

        string? answer = Console.ReadLine();

        if (answer?.Equals(
            "Y", StringComparison.OrdinalIgnoreCase) == true)
        {
            Console.WriteLine();
            Console.WriteLine("Qwen is analyzing the data...");

            string summary =
                await aiService.SummarizeAsync(data);

            Console.WriteLine();
            Console.WriteLine("AI Summary:");
            Console.WriteLine(summary);
        }
    }
    catch (Microsoft.Data.SqlClient.SqlException ex)
    {
        Console.WriteLine(
            "Database error: " + ex.Message);
    }
    catch (HttpRequestException ex)
    {
        Console.WriteLine(
            "Ollama API error: " + ex.Message);
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine(
            "The request timed out.");
    }
    catch (Exception ex)
    {
        Console.WriteLine(
            "Error: " + ex.Message);
    }
}

Console.WriteLine("Goodbye!");

