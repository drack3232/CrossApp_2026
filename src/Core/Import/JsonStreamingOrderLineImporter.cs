
using System.Runtime.CompilerServices;
using System.Text.Json;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;

namespace Core.Import;

public sealed class JsonStreamingOrderLineImporter
    : IStreamingOrderLineImporter
{
    private readonly int _delayMs;

    public JsonStreamingOrderLineImporter(int delayMs = 0)
    {
        if (delayMs < 0)
            throw new ArgumentOutOfRangeException(nameof(delayMs));

        _delayMs = delayMs;
    }

    public async IAsyncEnumerable<OrderLineImportDto> StreamAsync(
        string path,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        await using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            4096,
            useAsync: true
        );

        int index = 0;

        await foreach (var item in JsonSerializer
            .DeserializeAsyncEnumerable<OrderLineImportDto>(
                stream,
                cancellationToken: ct
            ))
        {
            ct.ThrowIfCancellationRequested();
            index++;

            if (item is null)
            {
                throw new Core.DomainException(
                    $"Запис {index}: порожня позиція."
                );
            }

            try
            {
                if (string.IsNullOrWhiteSpace(item.OrderId))
                    throw new ArgumentException(
                        "ID замовлення обов'язковий."
                    );

                OrderLine.Create(
                    item.ProductId,
                    item.Name,
                    item.Price,
                    item.Quantity
                );
            }
            catch (ArgumentException ex)
            {
                throw new Core.DomainException(
                    $"Запис {index}: {ex.Message}",
                    ex
                );
            }

            if (_delayMs > 0)
                await Task.Delay(_delayMs, ct);

            yield return item;
        }
    }
}
