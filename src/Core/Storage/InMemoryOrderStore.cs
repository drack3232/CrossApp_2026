using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

public sealed class InMemoryOrderStore(IEnumerable<Order>? seed = null) : IOrderStore
{
    private readonly object _sync = new();
    private readonly Dictionary<string, Order> _items =
        (seed ?? []).ToDictionary(o => o.Id, StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<Order> List()
    {
        lock (_sync)
        {
            return _items.Values.ToList();
        }
    }

    public Order? GetById(string id)
    {
        lock (_sync)
        {
            return _items.GetValueOrDefault(id);
        }
    }

    public void Add(Order item)
    {
        ArgumentNullException.ThrowIfNull(item);
        lock (_sync)
        {
            if (_items.ContainsKey(item.Id))
                throw new InvalidOperationException($"Замовлення з id={item.Id} уже існує.");

            _items.Add(item.Id, item);
        }
    }

    public void Update(Order item)
    {
        ArgumentNullException.ThrowIfNull(item);
        lock (_sync)
        {
            _items[item.Id] = item;
        }
    }

    public bool Remove(string id)
    {
        lock (_sync)
        {
            return _items.Remove(id);
        }
    }
}