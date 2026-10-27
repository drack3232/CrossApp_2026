using Core.Domain;

namespace Core.Reports;

public static class ReportService
{
    public static IReadOnlyList<CustomerTotalLine> TopCustomers(
        IEnumerable<Order> source,
        int n)
    {
        return source
            .GroupBy(o => o.Customer, StringComparer.OrdinalIgnoreCase)
            .Select(g => new CustomerTotalLine(
                g.Key,
                g.Count(),
                g.Sum(o => o.Total)
            ))
            .OrderByDescending(x => x.TotalAmount)
            .ThenBy(x => x.Customer, StringComparer.Ordinal)
            .Take(n)
            .ToList();
    }

    public static IReadOnlyList<OrderAmountLine> ExpensiveOrders(
        IEnumerable<Order> source,
        decimal threshold)
    {
        return source
            .Where(o => o.Total > threshold)
            .OrderByDescending(o => o.Total)
            .ThenBy(o => o.Id, StringComparer.Ordinal)
            .Select(o => new OrderAmountLine(
                o.Id,
                o.Customer,
                o.Total,
                o.IsConfirmed
            ))
            .ToList();
    }

    public static IReadOnlyList<StatusSummary> ByStatus(
        IEnumerable<Order> source)
    {
        return source
            .GroupBy(o => o.IsConfirmed)
            .Select(g => new StatusSummary(
                g.Key ? "Підтверджено" : "Не підтверджено",
                g.Count(),
                g.Sum(o => o.Total)
            ))
            .OrderBy(x => x.Status, StringComparer.Ordinal)
            .ToList();
    }

    public static OrderTotals Totals(
        IEnumerable<Order> source)
    {
        var orders = source.ToList();

        if (orders.Count == 0)
        {
            return new OrderTotals(0, 0m, 0m, 0m);
        }

        return new OrderTotals(
            orders.Count,
            orders.Sum(o => o.Total),
            orders.Average(o => o.Total),
            orders.Max(o => o.Total)
        );
    }

    public static (
        int Before,
        int After,
        int SnapshotAfterClear,
        int QueryAfterClear
    ) DeferredDemo(IEnumerable<Order> orders)
    {
        var source = orders.ToList();

        var query = source.Where(o => o.Total > 1000m);

        int before = query.Count();

        var extra = Order.Create(
            "L7-DEMO",
            "Демо клієнт",
            "Львів"
        );

        extra.AddLine(
            "L7-P1",
            "Тестовий товар",
            2000m,
            1
        );

        source.Add(extra);

        int after = query.Count();

        var snapshot = query.ToList();

        source.Clear();

        return (
            before,
            after,
            snapshot.Count,
            query.Count()
        );
    }
}