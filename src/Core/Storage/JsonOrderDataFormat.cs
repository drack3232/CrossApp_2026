
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

public sealed record StoredLineDto(
    string ProductId,
    string ProductName,
    decimal Price,
    int Quantity
);

public sealed record StoredOrderDto(
    string Id,
    string Customer,
    string Address,
    string? Comment,
    bool IsConfirmed,
    List<StoredLineDto> Lines
);

public sealed class JsonOrderDataFormat : IOrderDataFormat
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public void Export(string path, IEnumerable<Order> orders)
    {
        ArgumentNullException.ThrowIfNull(orders);

        string fullPath = Path.GetFullPath(path);

        Directory.CreateDirectory(
            Path.GetDirectoryName(fullPath)!
        );

        var data = orders
            .OrderBy(o => o.Id, StringComparer.Ordinal)
            .Select(o => new StoredOrderDto(
                o.Id,
                o.Customer,
                o.Address,
                o.Comment,
                o.IsConfirmed,
                o.Lines.Select(line => new StoredLineDto(
                    line.ProductId,
                    line.Name,
                    line.Price,
                    line.Quantity
                )).ToList()
            ))
            .ToList();

        string temporaryPath = fullPath + ".tmp";

        try
        {
            string json = JsonSerializer.Serialize(data, Options);

            File.WriteAllText(
                temporaryPath,
                json,
                new UTF8Encoding(false)
            );

            File.Move(
                temporaryPath,
                fullPath,
                overwrite: true
            );
        }
        finally
        {
            if (File.Exists(temporaryPath))
                File.Delete(temporaryPath);
        }
    }

    public IReadOnlyList<Order> Import(string path)
    {
        string fullPath = Path.GetFullPath(path);

        string json = File.ReadAllText(
            fullPath,
            Encoding.UTF8
        );

        var data = JsonSerializer.Deserialize<
            List<StoredOrderDto?>
        >(json, Options)
            ?? throw new InvalidDataException(
                "JSON не містить списку замовлень."
            );

        var orders = new List<Order>();

        var ids = new HashSet<string>(
            StringComparer.OrdinalIgnoreCase
        );

        foreach (var item in data)
        {
            if (item is null)
                throw new InvalidDataException(
                    "JSON містить порожній запис."
                );

            var order = Order.Create(
                item.Id,
                item.Customer,
                item.Address,
                item.Comment
            );

            if (!ids.Add(order.Id))
                throw new InvalidDataException(
                    $"Повторний ID: {order.Id}"
                );

            if (item.Lines is null)
                throw new InvalidDataException(
                    $"Замовлення {order.Id} не містить масиву lines."
                );

            foreach (var line in item.Lines)
            {
                order.AddLine(
                    line.ProductId,
                    line.ProductName,
                    line.Price,
                    line.Quantity
                );
            }

            if (item.IsConfirmed)
                order.Confirm();

            orders.Add(order);
        }

        return orders;
    }
}
