
using Core.Abstractions;
using Core.Domain;
using Core.Dto;
using Core.Services;
using Core.Storage;
using Xunit;

namespace Core.Tests;

public class ImportTests
{
    private sealed class FakeImporter : IOrderLineImporter
    {
        public Task<IReadOnlyList<OrderLineImportDto>> LoadAsync(
            string path,
            CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            IReadOnlyList<OrderLineImportDto> lines =
                new List<OrderLineImportDto>
                {
                    new(
                        "T-301",
                        "P-301",
                        "Срібний меч",
                        500m,
                        2
                    )
                };

            return Task.FromResult(lines);
        }
    }

    [Fact]
    public async Task ImportAsync_AddsLinesWithoutDuplicates()
    {
        var order = Order.Create(
            "T-301",
            "Геральт",
            "Львів"
        );

        var store = new InMemoryOrderStore(
            new List<Order> { order }
        );

        var service = new OrderService(store);
        var importer = new FakeImporter();

        int first = await service.ImportAsync(
            importer,
            "unused.json"
        );

        int second = await service.ImportAsync(
            importer,
            "unused.json"
        );

        Assert.Equal(1, first);
        Assert.Equal(0, second);
        Assert.Single(service.Find("T-301")!.Lines);
        Assert.Equal(1000m, service.Find("T-301")!.Total);
    }

    [Fact]
    public async Task ImportAsync_WhenCancelled_Throws()
    {
        var store = new InMemoryOrderStore(
            new List<Order>()
        );

        var service = new OrderService(store);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.ImportAsync(
                new FakeImporter(),
                "unused.json",
                cts.Token
            )
        );
    }
}
