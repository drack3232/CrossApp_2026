namespace Core.Reports;

public sealed record OrderTotals(
    int OrdersCount,
    decimal TotalAmount,
    decimal AverageAmount,
    decimal MaxAmount
);