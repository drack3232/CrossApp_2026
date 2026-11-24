namespace Infrastructure.Persistence;

public sealed class OrderRow
{
    public string Id { get; set; } = "";
    public string Customer { get; set; } = "";
    public string Address { get; set; } = "";
    public string? Comment { get; set; }
    public bool IsConfirmed { get; set; }
public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow; // ++1
    public List<OrderLineRow> Lines { get; set; } = [];
}