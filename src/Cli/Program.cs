using System.Text;
using Core.Domain;

Console.OutputEncoding = Encoding.UTF8;

Console.WriteLine("Сценарій 1: успіх");
Order order = Order.Create("O-001", "Відьмак", "Каер Морген", "Терміново");
order.AddLine("P-01", "Срібний меч", 1500.00m, 1);
order.AddLine("P-02", "Еліксир Ластівка", 50.50m, 5);
Console.WriteLine(order);
order.Confirm();
Console.WriteLine(order);
Console.WriteLine();

Console.WriteLine("Сценарій 2: порушення інваріантів");

TryDo("Додавання до підтвердженого", () => order.AddLine("P-03", "Зілля", 20m, 1));

TryDo("Порожній ID замовлення", () => Order.Create("", "Лютік", "Оксенфурт"));

Order newOrder = Order.Create("O-002", "Геральт", "Новіград");
TryDo("Від'ємна ціна в рядку", () => newOrder.AddLine("P-04", "Обладунки", -100m, 1));

TryDo("Підтвердження порожнього замовлення", () => newOrder.Confirm());

static void TryDo(string title, Action action)
{
    try
    {
        action();
        Console.WriteLine($" {title}: виняток НЕ спрацював — інваріант відсутній!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($" {title}: {ex.GetType().Name} - {ex.Message}");
    }
}