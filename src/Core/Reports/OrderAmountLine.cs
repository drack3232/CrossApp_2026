namespace Core.Reports;

public sealed record OrderAmountLine(
    string Id,
    string Customer,
    decimal Total,
    bool IsConfirmed
);