
using Core.Abstractions;
using Core.Domain;

namespace Core.Services;

public sealed class OrderService
{
    private readonly IOrderStore _store;

    public OrderService(IOrderStore store)
    {
        _store = store
            ?? throw new ArgumentNullException(nameof(store));
    }

    public Order CreateOrder(
        string id,
        string customer,
        string address,
        string? comment = null)
    {
        var order = Order.Create(id, customer, address, comment);

        _store.Add(order);

        return order;
    }

    public void AddLine(
        string orderId,
        string productId,
        string productName,
        decimal price,
        int quantity)
    {
        var order = _store.GetById(orderId)
            ?? throw new InvalidOperationException(
                $"Замовлення з ID={orderId} не знайдено.");

        order.AddLine(
            productId,
            productName,
            price,
            quantity
        );

        _store.Update(order);
    }

    public void AddLine(
        string orderId,
        string productName,
        decimal price,
        int quantity)
    {
        string productId = Guid.NewGuid().ToString("N")[..8];

        AddLine(
            orderId,
            productId,
            productName,
            price,
            quantity
        );
    }

    public void Confirm(string orderId)
    {
        var order = _store.GetById(orderId)
            ?? throw new InvalidOperationException(
                $"Замовлення з ID={orderId} не знайдено.");

        order.Confirm();

        _store.Update(order);
    }

    public Order? Find(string id)
    {
        return _store.GetById(id);
    }

    public IReadOnlyList<Order> All()
    {
        return _store.List();
    }
}
