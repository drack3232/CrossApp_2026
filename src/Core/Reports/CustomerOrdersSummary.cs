namespace Core.Reports;

public sealed record CustomerOrdersSummary(
    string Customer,
    int OrdersCount,
    decimal TotalAmount
);