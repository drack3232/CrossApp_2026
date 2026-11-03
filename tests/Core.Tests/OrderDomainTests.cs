
using Core;
using Core.Domain;

namespace Core.Tests;

public class OrderDomainTests
{
    [Fact]
    public void AddLine_ToConfirmedOrder_ThrowsDomainException()
    {
        var order = Order.Create(
            "O-001",
            "Геральт",
            "Каер Морхен"
        );

        order.AddLine("P-001", "Меч", 1000m, 1);
        order.Confirm();

        decimal totalBefore = order.Total;

        Assert.Throws<DomainException>(() =>
            order.AddLine("P-002", "Зілля", 100m, 2)
        );

        Assert.Equal(totalBefore, order.Total);
        Assert.Single(order.Lines);
    }
}
