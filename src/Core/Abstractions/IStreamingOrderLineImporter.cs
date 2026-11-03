using Core.Dto;

namespace Core.Abstractions;

public interface IStreamingOrderLineImporter
{
    IAsyncEnumerable<OrderLineImportDto> StreamAsync(
        string path,
        CancellationToken ct = default
    );
}