
using Core.Domain;
using Core.Events;
using Core.Services;
using Core.Storage;
using Xunit;

namespace Core.Tests;

public class OrderServiceTests
{
    [Fact]
    public void AddLine_UpdatesOrderTotal()
    {
       
        var order = Order.Create("T-001", "Геральт", "Львів");

        order.AddLine("P-001", "Меч", 500m, 2);
        order.AddLine("P-002", "Зілля", 100m, 3);

        Assert.Equal(1300m, order.Total);
        Assert.Equal(2, order.Lines.Count);
    }

    [Fact]
    public void CreateOrder_RaisesEventExactlyOnce()
    {

        var store = new InMemoryOrderStore(new List<Order>());
        var service = new OrderService(store);

        var events = new List<OrderChangedEventArgs>();

        service.Changed += (_, e) => events.Add(e);

        service.CreateOrder("T-002", "Лютик", "Львів");

        Assert.Single(events);
        Assert.Equal(
            OrderChangeKind.OrderCreated,
            events[0].Kind
        );
    }

    [Fact]
    public void Confirm_EmptyOrder_ThrowsDomainException()
    {
        
        var store = new InMemoryOrderStore(new List<Order>());
        var service = new OrderService(store);

        service.CreateOrder("T-003", "Йеннефер", "Львів");

        Assert.Throws<DomainException>(
            () => service.Confirm("T-003")
        );

        Assert.False(service.Find("T-003")!.IsConfirmed);
    }
}
