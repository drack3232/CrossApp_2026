namespace Core.Reports;

public sealed record CustomerTotalLine(
    string Customer,
    int OrdersCount,
    decimal TotalAmount
);