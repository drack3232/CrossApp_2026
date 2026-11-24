using Core.Abstractions;
using Core.Domain;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public sealed class EfOrderStore(AppDbContext db) : IOrderStore
{
    private static Order ToDomain(OrderRow row)
    {
        var order = Order.Create(row.Id, row.Customer, row.Address, row.Comment);
        foreach (var line in row.Lines)
        {
            order.AddLine(line.ProductId, line.Name, line.Price, line.Quantity);
        }

        if (row.IsConfirmed)
        {
            order.Confirm();
        }

        return order;
    }

    private static OrderRow ToRow(Order order) => new()
    {
        Id = order.Id,
        Customer = order.Customer,
        Address = order.Address,
        Comment = order.Comment,
        IsConfirmed = order.IsConfirmed,
        Lines = order.Lines.Select(l => new OrderLineRow
        {
            OrderId = order.Id,
            ProductId = l.ProductId,
            Name = l.Name,
            Price = l.Price,
            Quantity = l.Quantity
        }).ToList()
    };

    public IReadOnlyList<Order> List() =>
        db.Orders
            .Include(o => o.Lines)
            .AsNoTracking()
            .OrderBy(o => o.Id)
            .AsEnumerable()
            .Select(ToDomain)
            .ToList();

    public Order? GetById(string id) =>
        db.Orders
            .Include(o => o.Lines)
            .AsNoTracking()
            .SingleOrDefault(o => o.Id == id) is { } row
            ? ToDomain(row)
            : null;

    public void Add(Order item)
    {
        db.Orders.Add(ToRow(item));
        db.SaveChanges();
    }

    public void Update(Order item)
    {
        var row = db.Orders
            .Include(o => o.Lines)
            .Single(o => o.Id == item.Id);

        row.Customer = item.Customer;
        row.Address = item.Address;
        row.Comment = item.Comment;
        row.IsConfirmed = item.IsConfirmed;

        db.OrderLines.RemoveRange(row.Lines);
        row.Lines = item.Lines.Select(l => new OrderLineRow
        {
            OrderId = item.Id,
            ProductId = l.ProductId,
            Name = l.Name,
            Price = l.Price,
            Quantity = l.Quantity
        }).ToList();

        db.SaveChanges();
    }

    public bool Remove(string id)
    {
        var row = db.Orders.SingleOrDefault(o => o.Id == id);
        if (row is null)
            return false;

        db.Orders.Remove(row);
        db.SaveChanges();
        return true;
    }
}