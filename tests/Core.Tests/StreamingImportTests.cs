
using System.Runtime.CompilerServices;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;
using Core.Events;
using Core.Import;
using Core.Services;
using Core.Storage;
using Xunit;

namespace Core.Tests;

public class StreamingImportTests
{
    private sealed class FakeStreamingImporter
        : IStreamingOrderLineImporter
    {
        public async IAsyncEnumerable<OrderLineImportDto> StreamAsync(
            string path,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            yield return new OrderLineImportDto(
                "T-501", "P-501", "Меч", 100m, 2
            );

            await Task.Yield();
            ct.ThrowIfCancellationRequested();

            yield return new OrderLineImportDto(
                "T-501", "P-502", "Зілля", 50m, 1
            );
        }
    }

    private sealed class SlowStreamingImporter
        : IStreamingOrderLineImporter
    {
        public async IAsyncEnumerable<OrderLineImportDto> StreamAsync(
            string path,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            await Task.Delay(500, ct);

            yield return new OrderLineImportDto(
                "T-502", "P-503", "Товар", 100m, 1
            );
        }
    }

    private sealed class TestProgress : IProgress<int>
    {
        public List<int> Values { get; } = new();

        public void Report(int value)
        {
            Values.Add(value);
        }
    }

    [Fact]
    public async Task StreamingImport_ReportsProgressAndRaisesEvents()
    {
        var order = Order.Create("T-501", "Геральт", "Львів");

        var store = new InMemoryOrderStore(
            new List<Order> { order }
        );

        var service = new OrderService(store);
        var progress = new TestProgress();

        int eventsCount = 0;

        service.Changed += (_, e) =>
        {
            if (e.Kind == OrderChangeKind.LineAdded)
                eventsCount++;
        };

        int added = await service.ImportStreamingAsync(
            new FakeStreamingImporter(),
            "unused.json",
            2,
            progress
        );

        Assert.Equal(2, added);
        Assert.Equal(2, eventsCount);
        Assert.Equal(new[] { 0, 50, 100 }, progress.Values);
        Assert.Equal(250m, service.Find("T-501")!.Total);
    }

    [Fact]
    public async Task StreamingImport_TimeoutCancelsLinkedToken()
    {
        var order = Order.Create("T-502", "Лютик", "Львів");

        var service = new OrderService(
            new InMemoryOrderStore(new List<Order> { order })
        );

        using var userCts = new CancellationTokenSource();
        using var timeoutCts = new CancellationTokenSource();

        using var linkedCts =
            CancellationTokenSource.CreateLinkedTokenSource(
                userCts.Token,
                timeoutCts.Token
            );

        timeoutCts.CancelAfter(30);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.ImportStreamingAsync(
                new SlowStreamingImporter(),
                "unused.json",
                1,
                null,
                linkedCts.Token
            )
        );

        Assert.True(timeoutCts.IsCancellationRequested);
        Assert.False(userCts.IsCancellationRequested);
        Assert.Empty(service.Find("T-502")!.Lines);
    }

    [Fact]
    public async Task StreamingImport_InvalidRecordThrowsDomainException()
    {
        string path = Path.Combine(
            Path.GetTempPath(),
            $"lab08-{Guid.NewGuid():N}.json"
        );

        await File.WriteAllTextAsync(
            path,
            """
            [
              {
                "OrderId": "T-501",
                "ProductId": "P-BAD",
                "Name": "Некоректний товар",
                "Price": 100,
                "Quantity": -1
              }
            ]
            """
        );

        try
        {
            var importer = new JsonStreamingOrderLineImporter();

            await Assert.ThrowsAsync<Core.DomainException>(
                async () =>
                {
                    await foreach (var item in importer.StreamAsync(path))
                    {
                        _ = item;
                    }
                }
            );
        }
        finally
        {
            File.Delete(path);
        }
    }
}
