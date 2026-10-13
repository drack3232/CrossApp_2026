namespace Core.Dto;

public record OrderDto(
    string Id,
    string Customer,
    string Address,
    decimal Price,
    string? Comment = null,
    List<OrderLineDto>? Lines = null,
    bool IsConfirmed = false
    );
    
    public record OrderLineDto(
    string ProductId,
    string Name,
    decimal Price,
    int Quantity
);