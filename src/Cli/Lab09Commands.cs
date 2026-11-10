using Core;
using Core.Services;
using Cli.Configuration;
using Core.Abstractions;
using Core.Domain;
using Core.Storage;

namespace Cli;

public static class Lab09Commands
{
    public static bool TryHandle(
        string[] args,
        RuntimeConfiguration runtime)
    {
        string? command = args.FirstOrDefault(
            a => a is "export" or "import-all"
        );

        if (command is null)
            return false;

    // IOrderDataFormat format = new JsonOrderDataFormat();

    IOrderDataFormat format =
    runtime.Options.Storage.Format.ToLowerInvariant() switch
    {
        "csv" => new CsvOrderDataFormat(),
        "json" => new JsonOrderDataFormat(),
        _ => throw new ArgumentException("Невідомий формат.")
    };

        try
        {
            if (command == "export")
            {
                IReadOnlyList<Order> orders = args.Contains("--file")
                    ? new FileOrderStore(runtime.JsonStorePath).List()
                    : SampleData.Orders();

                format.Export(runtime.DataFilePath, orders);

                Console.WriteLine(
                    $"Експортовано {orders.Count} замовлень."
                );
            }
            else
            {
                var orders = format.Import(runtime.DataFilePath);

                var store = new InMemoryOrderStore(orders);
                var imported = store.List();

                Console.WriteLine(
                    $"Імпортовано {imported.Count} замовлень."
                );

                Console.WriteLine(
                    $"Загальна сума: {imported.Sum(o => o.Total):F2} " +
                    runtime.Options.Orders.Currency
                );

                Console.WriteLine(
                    $"Кількість позицій: " +
                    imported.Sum(o => o.Lines.Count)
                );
            }
        }
        catch (Exception ex) when (
            ex is IOException or
            UnauthorizedAccessException or
            System.Text.Json.JsonException or
            ArgumentException or
            Core.DomainException)
        {
            Console.Error.WriteLine(
                $"Помилка імпорту/експорту: {ex.Message}"
            );

            Environment.ExitCode = 1;
        }

        return true;
    }
}
