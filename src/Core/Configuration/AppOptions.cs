
namespace Core.Configuration;

public sealed class AppOptions
{
    public string EnvironmentName { get; set; } = "Development";
    public StorageOptions Storage { get; set; } = new();
    public LoggingOptions Logging { get; set; } = new();
    public OrdersOptions Orders { get; set; } = new();
}

public sealed class StorageOptions
{
    public string DataPath { get; set; } = "data";
    public string FileName { get; set; } = "orders.json";
    public string Format { get; set; } = "json";

    public string ResolveFullPath(string baseDirectory)
    {
        string directory = Path.IsPathRooted(DataPath)
            ? DataPath
            : Path.Combine(baseDirectory, DataPath);

        directory = Path.GetFullPath(directory);

        Directory.CreateDirectory(directory);

        return Path.GetFullPath(
            Path.Combine(directory, FileName)
        );
    }
}

public sealed class LoggingOptions
{
    public string MinimumLevel { get; set; } = "Information";
}

public sealed class OrdersOptions
{
    public string Currency { get; set; } = "UAH";
}

public static class AppOptionsValidator
{
    public static void Validate(AppOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(options.Storage.DataPath))
            throw new ArgumentException(
                "Storage:DataPath не може бути порожнім."
            );

        string fileName = options.Storage.FileName;

        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException(
                "Storage:FileName не може бути порожнім."
            );

        if (Path.GetFileName(fileName) != fileName)
            throw new ArgumentException(
                "Storage:FileName має бути іменем файлу."
            );

        string format = options.Storage.Format
            .Trim()
            .ToLowerInvariant();

        if (format != "json" && format != "csv")
            throw new ArgumentException(
                "Storage:Format має бути json або csv."
            );

        string extension = format == "json" ? ".json" : ".csv";

        if (!fileName.EndsWith(
                extension,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                $"Storage:FileName має закінчуватися на {extension}."
            );
        }

        if (string.IsNullOrWhiteSpace(options.Orders.Currency))
            throw new ArgumentException(
                "Orders:Currency не може бути порожнім."
            );
    }
}
