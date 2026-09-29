using System.Globalization;
using Core.Dto;

namespace Core.Import;

public static class OrderCsvImporter
{
    private const char Separator = ';';

    public static ImportResult<OrderDto> Load(string path)
    {
        var items = new List<OrderDto>();
        var errors = new List<string>();
        string[] lines = File.ReadAllLines(path);

        for (int i = 0; i < lines.Length; i++)
        {
            int number = i + 1;
            string line = lines[i];

            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                continue;

            if (number == 1 && line.StartsWith("id", StringComparison.OrdinalIgnoreCase))
                continue;

            switch (ParseLine(line))
            {
                case ParseOk ok:
                    items.Add(ok.Value);
                    break;
                case ParseFailed failed:
                    errors.Add($"рядок {number}: {failed.Reason}");
                    break;
            }
        }

        return new ImportResult<OrderDto>(items, errors);
    }

    private static ParseOutcome ParseLine(string line)
    {
        string[] parts = line.Split(Separator, StringSplitOptions.TrimEntries);

        return parts switch
        {
            { Length: < 4 } => new ParseFailed($"очікую щонайменше 4 колонки, отримав {parts.Length}"),
            [_, "", _, _, ..] => new ParseFailed("Ім'я клієнта порожнє"),
            [_, _, "", _, ..] => new ParseFailed("Адреса порожня"),
            [_, _, _, var priceStr, ..] when !decimal.TryParse(priceStr, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal price) || price < 0
                => new ParseFailed($"ціна '{priceStr}' не є невід'ємним числом"),
            [var id, var customer, var address, var priceStr] 
                => new ParseOk(new OrderDto(id, customer, address, decimal.Parse(priceStr, CultureInfo.InvariantCulture))),
            [var id, var customer, var address, var priceStr, var comment] 
                => new ParseOk(new OrderDto(id, customer, address, decimal.Parse(priceStr, CultureInfo.InvariantCulture), string.IsNullOrWhiteSpace(comment) ? null : comment)),
            _ => new ParseFailed($"занадто багато колонок: {parts.Length}")
        };
    }

    private abstract record ParseOutcome;
    private sealed record ParseOk(OrderDto Value) : ParseOutcome;
    private sealed record ParseFailed(string Reason) : ParseOutcome;
}