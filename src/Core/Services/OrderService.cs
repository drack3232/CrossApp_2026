
using Core.Abstractions;
using Core.Domain;
using Core.Events;

namespace Core.Services;

public sealed class OrderService
{
    private readonly IOrderStore _store;
    private readonly INotificationSink? _notificationSink;

    public OrderService(IOrderStore store,
    INotificationSink? notificationSink = null)
    {
        _store = store
            ?? throw new ArgumentNullException(nameof(store));
             _notificationSink = notificationSink;
    }

    public event EventHandler<OrderChangedEventArgs>? Changed;

    private void OnChanged(Order order, OrderChangeKind kind)
{
    var eventArgs = new OrderChangedEventArgs(
        order.Id,
        kind,
        order.Lines.Count,
        order.Total
    );

    Changed?.Invoke(this, eventArgs);

    _notificationSink?.Notify(eventArgs);
}

    public Order CreateOrder(
        string id,
        string customer,
        string address = "",
        string comment = "")
    {
        var order = Order.Create(id, customer, address, comment);

        _store.Add(order);

        OnChanged(order, OrderChangeKind.OrderCreated);

        return order;
    }

    public void AddLine(
        string orderId,
        string productName,
        decimal price,
        int quantity)
    {
        var order = _store.GetById(orderId)
            ?? throw new InvalidOperationException(
                $"Замовлення з id={orderId} не знайдено.");

        order.AddLine(
            Guid.NewGuid().ToString("N")[..8],
            productName,
            price,
            quantity
        );

        _store.Update(order);

        OnChanged(order, OrderChangeKind.LineAdded);
    }


    public void Confirm(string orderId)
    {
        var order = _store.GetById(orderId)
            ?? throw new InvalidOperationException(
                $"Замовлення з id={orderId} не знайдено.");

        order.Confirm();

        _store.Update(order);

        OnChanged(order, OrderChangeKind.OrderConfirmed);
    }


public async Task<int> ImportAsync(
    IOrderLineImporter importer,
    string path,
    CancellationToken ct = default)
{
    ArgumentNullException.ThrowIfNull(importer);

    var lines = await importer.LoadAsync(path, ct);
    int added = 0;

    foreach (var line in lines)
    {
        ct.ThrowIfCancellationRequested();

        var order = _store.GetById(line.OrderId)
            ?? throw new Core.DomainException(
                $"Замовлення {line.OrderId} не знайдено."
            );

        bool exists = order.Lines.Any(
            x => x.ProductId == line.ProductId
        );

        if (exists)
            continue;

        order.AddLine(
            line.ProductId,
            line.Name,
            line.Price,
            line.Quantity
        );

        _store.Update(order);

     
        OnChanged(order, OrderChangeKind.LineAdded);

        added++;
    }

    return added;
}

    public IReadOnlyList<Order> All() => _store.List();

    public Order? Find(string id) => _store.GetById(id);

    public IReadOnlyList<Order> FindAll(
        Func<Order, bool> predicate)
    {
        return _store.List()
            .Where(predicate)
            .ToList();
    }
}
