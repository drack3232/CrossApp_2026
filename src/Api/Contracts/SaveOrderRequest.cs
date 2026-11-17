namespace Api.Contracts;

public sealed record SaveOrderLineRequest(
    string ProductId,
    string Name,
    decimal Price,
    int Quantity);

public sealed record SaveOrderRequest(
    string Customer,
    string Address,
    string? Comment,
    List<SaveOrderLineRequest> Lines,
    bool IsConfirmed = false);

public sealed record OrderTotalResponse(
    string OrderId,
    string Customer,
    decimal Total,
    int LinesCount,
    bool IsConfirmed);