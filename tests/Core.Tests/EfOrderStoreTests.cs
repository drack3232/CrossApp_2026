using Core.Domain;
using Infrastructure;
using Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Core.Tests;

public sealed class EfOrderStoreTests : IDisposable
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");
    private readonly AppDbContext _db;
    private readonly EfOrderStore _store;

    public EfOrderStoreTests()
    {
        _connection.Open();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _db = new AppDbContext(options);
        _db.Database.EnsureCreated();
        _store = new EfOrderStore(_db);
    }

    [Fact]
    public void Add_And_GetById_RestoresDomainOrderWithLines()
    {
        var order = Order.Create("ord-01", "Геральт", "Каер Морхен", "Тест");
        order.AddLine("P-01", "Срібний меч", 1500m, 1);
        order.Confirm();

        _store.Add(order);

        var loaded = _store.GetById("ord-01");
        Assert.NotNull(loaded);
        Assert.Equal("Геральт", loaded.Customer);
        Assert.True(loaded.IsConfirmed);
        Assert.Single(loaded.Lines);
        Assert.Equal(1500m, loaded.Lines.Sum(l => l.Price * l.Quantity));
    }

    [Fact]
    public void Update_ModifiesOrderAndReplacesLines()
    {
        var order = Order.Create("ord-02", "Йеннефер", "Венгерберг", null);
        order.AddLine("P-02", "Зілля", 200m, 1);
        _store.Add(order);

        var updated = Order.Create("ord-02", "Йеннефер", "Новіград", "Оновлено");
        updated.AddLine("P-03", "Амулет", 500m, 2);
        _store.Update(updated);

        var loaded = _store.GetById("ord-02");
        Assert.NotNull(loaded);
        Assert.Equal("Новіград", loaded.Address);
        Assert.Single(loaded.Lines);
        Assert.Equal(1000m, loaded.Lines.Sum(l => l.Price * l.Quantity));
    }

    [Fact]
    public void Remove_DeletesOrderAndCascadesToOrderLines()
    {
        var order = Order.Create("ord-03", "Трісс", "Марібор", null);
        order.AddLine("P-04", "Книга", 300m, 1);
        _store.Add(order);

        var removed = _store.Remove("ord-03");

        Assert.True(removed);
        Assert.Null(_store.GetById("ord-03"));
        Assert.Empty(_db.OrderLines);
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }
}