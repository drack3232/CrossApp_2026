namespace Core.Dto;

public record OrderDto(
    string Id,
    string Customer,
    string Address,
    decimal Price,
    string? Comment = null);