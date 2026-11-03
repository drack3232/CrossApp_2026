using Core.Dto;

namespace Core.Abstractions;

public interface IOrderLineImporter
{
    Task<IReadOnlyList<OrderLineImportDto>> LoadAsync(
        string path,
        CancellationToken ct = default
    );
}