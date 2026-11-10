
using System.Globalization;
using System.Text;
using Core.Abstractions;
using Core.Domain;
using Microsoft.VisualBasic.FileIO;

namespace Core.Storage;

public sealed class CsvOrderDataFormat : IOrderDataFormat
{
    private static readonly string[] Header =
    [
        "OrderId", "Customer", "Address", "Comment",
        "IsConfirmed", "ProductId", "ProductName",
        "Price", "Quantity"
    ];

    public void Export(string path, IEnumerable<Order> orders)
    {
        ArgumentNullException.ThrowIfNull(orders);

        string fullPath = Path.GetFullPath(path);

        Directory.CreateDirectory(
            Path.GetDirectoryName(fullPath)!
        );

        string tempPath = fullPath + ".tmp";

        try
        {
            using (var writer = new StreamWriter(
                tempPath, false, new UTF8Encoding(false)))
            {
                WriteRow(writer, Header);

                foreach (var order in orders.OrderBy(o => o.Id))
                {
                    if (order.Lines.Count == 0)
                    {
                        WriteRow(writer,
                        [
                            order.Id,
                            order.Customer,
                            order.Address,
                            order.Comment ?? "",
                            order.IsConfirmed.ToString(),
                            "", "", "", ""
                        ]);

                        continue;
                    }

                    foreach (var line in order.Lines)
                    {
                        WriteRow(writer,
                        [
                            order.Id,
                            order.Customer,
                            order.Address,
                            order.Comment ?? "",
                            order.IsConfirmed.ToString(),
                            line.ProductId,
                            line.Name,
                            line.Price.ToString(
                                CultureInfo.InvariantCulture),
                            line.Quantity.ToString(
                                CultureInfo.InvariantCulture)
                        ]);
                    }
                }
            }

            File.Move(tempPath, fullPath, overwrite: true);
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    public IReadOnlyList<Order> Import(string path)
    {
        using var parser = new TextFieldParser(
            Path.GetFullPath(path), Encoding.UTF8)
        {
            TextFieldType = FieldType.Delimited,
            HasFieldsEnclosedInQuotes = true,
            TrimWhiteSpace = false
        };

        parser.SetDelimiters(";");

        string[] heading = parser.ReadFields()
            ?? throw new InvalidDataException(
                "CSV-файл порожній."
            );

        if (!heading.SequenceEqual(Header))
            throw new InvalidDataException(
                "Неправильний заголовок CSV."
            );

        var orders = new Dictionary<string, Order>(
            StringComparer.OrdinalIgnoreCase
        );

        var confirmations = new Dictionary<string, bool>(
            StringComparer.OrdinalIgnoreCase
        );

        int rowNumber = 1;

        while (!parser.EndOfData)
        {
            rowNumber++;

            string[] columns = parser.ReadFields()
                ?? throw new InvalidDataException(
                    $"Рядок {rowNumber} порожній."
                );

            if (columns.Length != Header.Length)
                throw new InvalidDataException(
                    $"Рядок {rowNumber}: неправильна кількість полів."
                );

            try
            {
                string id = columns[0];
                string customer = columns[1];
                string address = columns[2];

                string? comment = columns[3].Length == 0
                    ? null
                    : columns[3];

                bool confirmed = bool.Parse(columns[4]);

                if (!orders.TryGetValue(id, out var order))
                {
                    order = Order.Create(
                        id, customer, address, comment
                    );

                    orders.Add(order.Id, order);
                    confirmations.Add(order.Id, confirmed);
                }
                else if (
                    order.Customer != customer ||
                    order.Address != address ||
                    (order.Comment ?? "") != (comment ?? "") ||
                    confirmations[id] != confirmed)
                {
                    throw new InvalidDataException(
                        "Суперечливі дані замовлення."
                    );
                }

                bool emptyLine = columns
                    .Skip(5)
                    .All(string.IsNullOrEmpty);

                if (!emptyLine)
                {
                    decimal price = decimal.Parse(
                        columns[7],
                        CultureInfo.InvariantCulture
                    );

                    int quantity = int.Parse(
                        columns[8],
                        CultureInfo.InvariantCulture
                    );

                    order.AddLine(
                        columns[5],
                        columns[6],
                        price,
                        quantity
                    );
                }
            }
            catch (Exception ex) when (
                ex is ArgumentException or
                FormatException or
                InvalidDataException)
            {
                throw new InvalidDataException(
                    $"Рядок {rowNumber}: {ex.Message}", ex
                );
            }
        }

        foreach (var pair in orders)
        {
            if (confirmations[pair.Key])
                pair.Value.Confirm();
        }

        return orders.Values.ToList();
    }

    private static void WriteRow(
        TextWriter writer,
        IEnumerable<string> values)
    {
        writer.WriteLine(
            string.Join(";", values.Select(Escape))
        );
    }

    private static string Escape(string value)
    {
        if (value.IndexOfAny([';', '"', '\r', '\n']) == -1)
            return value;

        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }
}
