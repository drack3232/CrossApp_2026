
using Core.Domain;
using Core.Reports;
using Xunit;

namespace Core.Tests;

public class ReportServiceTests
{
    private static Order CreateOrder(
        string id,
        string customer,
        decimal amount)
    {
        var order = Order.Create(id, customer, "Львів");

        order.AddLine(
            $"P-{id}",
            "Тестовий товар",
            amount,
            1
        );

        return order;
    }

    [Theory]
    [InlineData(1000, 1)]
    [InlineData(2000, 0)]
    [InlineData(400, 2)]
    public void ExpensiveOrders_FiltersCorrectly(
        int threshold,
        int expected)
    {
        var orders = new[]
        {
            CreateOrder("T-101", "Геральт", 500m),
            CreateOrder("T-102", "Лютик", 1500m)
        };

        var result = ReportService.ExpensiveOrders(
            orders,
            threshold
        );

        Assert.Equal(expected, result.Count);
    }

    [Fact]
    public void TopCustomers_HasDeterministicOrder()
    {
        var orders = new[]
        {
            CreateOrder("T-201", "Лютик", 1000m),
            CreateOrder("T-202", "Геральт", 1000m)
        };

        var result = ReportService.TopCustomers(orders, 2);

        Assert.Equal(
            new[] { "Геральт", "Лютик" },
            result.Select(x => x.Customer)
        );
    }
}
