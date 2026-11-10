
using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

public sealed class FileOrderStore : IOrderStore
{
    private readonly Dictionary<string, Order> _cache =
        new(StringComparer.OrdinalIgnoreCase);

    private readonly JsonOrderDataFormat _format = new();
    private readonly string _path;
    private bool _loaded;

    public FileOrderStore(string path)
    {
        _path = Path.GetFullPath(path);
    }

    private void EnsureLoaded()
    {
        if (_loaded)
            return;

        if (File.Exists(_path))
        {
            var orders = _format.Import(_path);

            foreach (var order in orders)
                _cache[order.Id] = order;
        }

        _loaded = true;
    }

    private void Flush()
    {
        _format.Export(_path, _cache.Values);
    }

    public IReadOnlyList<Order> List()
    {
        EnsureLoaded();
        return _cache.Values.ToList();
    }

    public Order? GetById(string id)
    {
        EnsureLoaded();
        return _cache.GetValueOrDefault(id);
    }

    public void Add(Order item)
    {
        ArgumentNullException.ThrowIfNull(item);
        EnsureLoaded();

        if (_cache.ContainsKey(item.Id))
            throw new InvalidOperationException(
                $"Замовлення з id={item.Id} уже існує."
            );

        _cache.Add(item.Id, item);
        Flush();
    }

    public void Update(Order item)
    {
        ArgumentNullException.ThrowIfNull(item);
        EnsureLoaded();

        _cache[item.Id] = item;
        Flush();
    }

    public bool Remove(string id)
    {
        EnsureLoaded();

        if (!_cache.Remove(id))
            return false;

        Flush();
        return true;
    }
}
