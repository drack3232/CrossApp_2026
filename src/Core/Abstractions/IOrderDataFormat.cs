using Core.Domain;

namespace Core.Abstractions;

public interface IOrderDataFormat
{
    void Export(string path, IEnumerable<Order> orders);

    IReadOnlyList<Order> Import(string path);
}