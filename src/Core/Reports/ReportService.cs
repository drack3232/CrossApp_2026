
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
            .TopBy(
                x => x.TotalAmount,
                n,
                x => x.Customer
            )
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
            return new OrderTotals(0, 0m, 0m, 0m);

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

    // ++1
    public static IReadOnlyList<CustomerOrdersSummary> CustomersWithOrders(
        IEnumerable<string> customers,
        IEnumerable<Order> orders)
    {
        return customers
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .GroupJoin(
                orders,
                customer => customer,
                order => order.Customer,
                (customer, customerOrders) => new CustomerOrdersSummary(
                    customer,
                    customerOrders.Count(),
                    customerOrders.Sum(o => o.Total)
                ),
                StringComparer.OrdinalIgnoreCase
            )
            .OrderByDescending(x => x.TotalAmount)
            .ThenBy(x => x.Customer, StringComparer.Ordinal)
            .ToList();
    }

//++2
    public static IReadOnlyList<OrderAmountLine> TopOrders(
        IEnumerable<Order> source,
        int n)
    {
        return source
            .TopBy(
                o => o.Total,
                n,
                o => o.Id
            )
            .Select(o => new OrderAmountLine(
                o.Id,
                o.Customer,
                o.Total,
                o.IsConfirmed
            ))
            .ToList();
    }

    // ++3
    public static IReadOnlyList<StatusSummary> ByStatusQuerySyntax(
        IEnumerable<Order> source)
    {
        var query =
            from order in source
            group order by order.IsConfirmed into g
            select new StatusSummary(
                g.Key ? "Підтверджено" : "Не підтверджено",
                g.Count(),
                g.Sum(o => o.Total)
            );

        return query
            .OrderBy(x => x.Status, StringComparer.Ordinal)
            .ToList();
    }
}
