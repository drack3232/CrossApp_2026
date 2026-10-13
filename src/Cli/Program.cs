
using System.Text;
using Core;
using Core.Domain;
using Core.Abstractions;
using Core.Storage;
using Core.Services;

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

TryDo("Додавання до підтвердженого",
    () => order.AddLine("P-03", "Зілля", 20m, 1));

TryDo("Порожній ID замовлення",
    () => Order.Create("", "Лютік", "Оксенфурт"));

Order newOrder = Order.Create("O-002", "Геральт", "Новіград");

TryDo("Від'ємна ціна в рядку",
    () => newOrder.AddLine("P-04", "Обладунки", -100m, 1));

TryDo("Підтвердження порожнього замовлення",
    () => newOrder.Confirm());

Console.WriteLine(newOrder);

bool useFile = args.Contains("--file");

string dataPath = Path.Combine(
    AppContext.BaseDirectory,
    "data",
    "orders.json"
);

//DI manuale
IOrderStore store = useFile
    ? new FileOrderStore(dataPath)
    : new InMemoryOrderStore(SampleData.Orders());

var service = new OrderService(store);

Console.WriteLine($"Сховище: {store.GetType().Name}");
Console.WriteLine($"Початкова кількість: {service.All().Count}");


var created = service.CreateOrder(
    Guid.NewGuid().ToString("N")[..8],
    "Геральт",
    "Новіград"
);


service.AddLine(created.Id, "Срібний меч", 1500m, 1);


service.Confirm(created.Id);


var found = service.Find(created.Id);

if (found != null)
{
    Console.WriteLine("\nЗнайдене замовлення:");
    Console.WriteLine(found);

    foreach (var line in found.Lines)
    {
        Console.WriteLine(
            $"{line.Name} | {line.Price} грн | {line.Quantity} шт."
        );
    }
}


Console.WriteLine("\nСписок усіх замовлень:");

foreach (var item in service.All())
{
    Console.WriteLine(item);
}


TryDo("Неіснуючий ID",
    () => service.Confirm("UNKNOWN-ID"));



static void TryDo(string title, Action action)
{
    try
    {
        action();
        Console.WriteLine(
            $" {title}: виняток НЕ спрацював — інваріант відсутній!"
        );
    }
    catch (Exception ex)
    {
        Console.WriteLine(
            $" {title}: {ex.GetType().Name} - {ex.Message}"
        );
    }
}
