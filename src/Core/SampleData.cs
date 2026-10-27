
using Core.Domain;

namespace Core;

public static class SampleData
{
    public static List<Order> Orders()
    {
        var o1 = Order.Create(
            "O-101",
            "Геральт з Рівії",
            "Каер Морхен",
            "Термінове замовлення"
        );

        o1.AddLine("L-101", "Срібний меч", 1500.00m, 1);
        o1.AddLine("L-102", "Зілля 'Ластівка'", 150.00m, 3);

        var o2 = Order.Create(
            "O-102",
            "Лютик",
            "Оксенфурт",
            ""
        );

        o2.AddLine("L-103", "Лютня", 850.00m, 1);
        o2.Confirm();

        var o3 = Order.Create(
            "O-103",
            "Йеннефер",
            "Венгерберг",
            ""
        );

        o3.AddLine(
            "L-104",
            "Парфуми з бузку і аґрусу",
            450.00m,
            2
        );

        var orders = new List<Order> { o1, o2, o3 };

        // Початкові замовлення: O-104 ... O-115
        for (int i = 4; i <= 15; i++)
        {
            var order = Order.Create(
                $"O-{100 + i}",
                $"Клієнт {i}",
                "Новіград"
            );

            order.AddLine(
                $"L-{200 + i}",
                $"Товар {i}",
                100m + i * 10,
                i
            );

            if (i % 3 == 0)
            {
                order.Confirm();
            }

            orders.Add(order);
        }

        // Додаткові замовлення для лабораторної №7
        var additionalOrders = new[]
        {
            new
            {
                Id = "O-116",
                Customer = "Геральт з Рівії",
                Address = "Каер Морхен",
                Price = 4000m,
                Confirm = true
            },
            new
            {
                Id = "O-117",
                Customer = "Лютик",
                Address = "Оксенфурт",
                Price = 3000m,
                Confirm = false
            },
            new
            {
                Id = "O-118",
                Customer = "Йеннефер",
                Address = "Венгерберг",
                Price = 2200m,
                Confirm = false
            },
            new
            {
                Id = "O-119",
                Customer = "Клієнт 4",
                Address = "Новіград",
                Price = 7000m,
                Confirm = true
            }
        };

        foreach (var item in additionalOrders)
        {
            var order = Order.Create(
                item.Id,
                item.Customer,
                item.Address
            );

            order.AddLine(
                $"L-{item.Id}",
                "Додатковий товар",
                item.Price,
                1
            );

            if (item.Confirm)
            {
                order.Confirm();
            }

            orders.Add(order);
        }

        return orders;
    }
}
