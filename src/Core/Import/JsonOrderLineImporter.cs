
using System.Text.Json;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;

namespace Core.Import;

public sealed class JsonOrderLineImporter : IOrderLineImporter
{
    public async Task<IReadOnlyList<OrderLineImportDto>> LoadAsync(
        string path,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        await using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 4096,
            useAsync: true
        );

        var dtos = await JsonSerializer.DeserializeAsync<
            List<OrderLineImportDto?>
        >(
            stream,
            cancellationToken: ct
        ) ?? throw new Core.DomainException(
            "JSON-файл не містить списку позицій."
        );

        var result = new List<OrderLineImportDto>(dtos.Count);

        for (int i = 0; i < dtos.Count; i++)
        {
            ct.ThrowIfCancellationRequested();

            var line = dtos[i];

            if (line is null)
            {
                throw new Core.DomainException(
                    $"Запис {i + 1}: позиція порожня."
                );
            }

            try
            {
                if (string.IsNullOrWhiteSpace(line.OrderId))
                {
                    throw new ArgumentException(
                        "ID замовлення обов'язковий."
                    );
                }

                OrderLine.Create(
                    line.ProductId,
                    line.Name,
                    line.Price,
                    line.Quantity
                );
            }
            catch (ArgumentException ex)
            {
                throw new Core.DomainException(
                    $"Запис {i + 1}: {ex.Message}",
                    ex
                );
            }

            result.Add(line);
        }

        return result;
    }
}
