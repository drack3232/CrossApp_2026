
using System.Diagnostics;
using Core.Domain;

namespace Core.Reports;

public static class ReportBenchmarks
{
    public static (
        int Records,
        decimal Sum,
        double WithoutListMs,
        double WithListMs
    ) Compare(int count = 100_000)
    {
        if (count <= 0)
            throw new ArgumentOutOfRangeException(nameof(count));

        var source = Enumerable.Range(1, count)
            .Select(i =>
            {
                var order = Order.Create(
                    $"BENCH-{i}",
                    $"Клієнт {i % 100}",
                    "Львів"
                );

                order.AddLine(
                    $"PRODUCT-{i}",
                    "Тестовий товар",
                    (i % 5000) + 1m,
                    1
                );

                return order;
            })
            .ToList();

        decimal WithoutList() =>
            source
                .Where(o => o.Total > 2000m)
                .Select(o => o.Total)
                .Sum();

        decimal WithList() =>
            source
                .Where(o => o.Total > 2000m)
                .ToList()
                .Select(o => o.Total)
                .Sum();

        WithoutList();
        WithList();

        const int repetitions = 5;

        double totalWithout = 0;
        double totalWith = 0;

        decimal resultWithout = 0;
        decimal resultWith = 0;

        for (int i = 0; i < repetitions; i++)
        {
            var stopwatch = Stopwatch.StartNew();

            resultWithout = WithoutList();

            stopwatch.Stop();
            totalWithout += stopwatch.Elapsed.TotalMilliseconds;

            stopwatch.Restart();

            resultWith = WithList();

            stopwatch.Stop();
            totalWith += stopwatch.Elapsed.TotalMilliseconds;
        }

        if (resultWithout != resultWith)
            throw new InvalidOperationException(
                "Результати двох LINQ-запитів не збігаються."
            );

        return (
            count,
            resultWithout,
            totalWithout / repetitions,
            totalWith / repetitions
        );
    }
}
