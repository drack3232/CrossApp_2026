using Core.Dto;

namespace Core.Domain;

public sealed class Order
{
    private readonly List<OrderLine> _lines = [];
    
    public string Id { get; }
    public string Customer { get; }
    public string Address { get; }
    public string? Comment { get; }
    public bool IsConfirmed { get; private set; }


    public IReadOnlyList<OrderLine> Lines => _lines.AsReadOnly();
    
    public decimal Total => _lines.Sum(l => l.Price * l.Quantity);

    private Order(string id, string customer, string address, string? comment)
    {
        Id = id;
        Customer = customer;
        Address = address;
        Comment = comment;
        IsConfirmed = false;
    }

    public static Order Create(string id, string customer, string address, string? comment = null)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Ідентифікатор замовлення обов'язковий", nameof(id));
            
        if (string.IsNullOrWhiteSpace(customer))
            throw new ArgumentException("Ім'я клієнта обов'язкове", nameof(customer));
            
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Адреса обов'язкова", nameof(address));

        return new Order(id.Trim(), customer.Trim(), address.Trim(), comment?.Trim());
    }


    public void AddLine(string productId, string name, decimal price, int quantity)
    {
        if (IsConfirmed)
            throw new InvalidOperationException($"Замовлення {Id} вже підтверджене, рядки додавати не можна");

        var line = OrderLine.Create(productId, name, price, quantity);
        _lines.Add(line);
    }


    public void Confirm()
    {
        if (_lines.Count == 0)
            throw new InvalidOperationException($"Неможливо підтвердити порожнє замовлення {Id}");
            
        IsConfirmed = true;
    }

    public OrderDto ToDto() => new(Id, Customer, Address, Total, Comment);

    public static Order FromDto(OrderDto dto)
    {
        return Create(dto.Id, dto.Customer, dto.Address, dto.Comment);
    }

    public override string ToString() => 
        $"{Id} [{Customer}] - Позицій: {Lines.Count}, Сума: {Total:C}, Підтверджено: {IsConfirmed}";
}