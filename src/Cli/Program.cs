
using System.Text;
using Core;
using Core.Domain;
using Core.Abstractions;
using Core.Storage;
using Core.Services;
using Core.Events;
using Cli.Notifications;
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

//+1
string notificationPath = Path.Combine(
    AppContext.BaseDirectory,
    "logs",
    "notifications.log"
);

INotificationSink? notificationSink = null;

if (args.Contains("--notify-console"))
{
    notificationSink = new ConsoleSink();
}
else if (args.Contains("--notify-file"))
{
    notificationSink = new FileSink(notificationPath);
}

var service = new OrderService(store, notificationSink);


Console.WriteLine($"Сховище: {store.GetType().Name}");
Console.WriteLine($"Початкова кількість: {service.All().Count}");


var created = service.CreateOrder(
    Guid.NewGuid().ToString("N")[..8],
    "Arthas",
    "lviv"
);


service.AddLine(created.Id, "Tesla Model 3", 18000m, 1);


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

string logPath = Path.Combine(
    AppContext.BaseDirectory,
    "logs",
    "app.log"
);

Directory.CreateDirectory(
    Path.GetDirectoryName(logPath)!
);

void OnOrderChanged(object? sender, OrderChangedEventArgs e)
{
    string line =
        $"{e.At:HH:mm:ss} {e.Kind,-15} " +
        $"{e.OrderId} | Позицій: {e.LinesCount} | " +
        $"Сума: {e.Total:0.00}";

    Console.WriteLine(line);

    File.AppendAllText(
        logPath,
        line + Environment.NewLine
    );
}

service.Changed += OnOrderChanged;

int changes = 0;

service.Changed += (_, _) => changes++;

var created6 = service.CreateOrder(
    "L6-" + Guid.NewGuid().ToString("N")[..8],
    "Цірі",
    "Каер Морхен"
);

service.AddLine(
    created6.Id,
    "Срібний меч",
    1500m,
    1
);

service.Confirm(created6.Id);

Console.WriteLine($"Подій отримано: {changes}");

TryDo(
    "Неіснуючий ID",
    () => service.Confirm("UNKNOWN-ID")
);

Console.WriteLine($"Подій після помилки: {changes}");

service.Changed -= OnOrderChanged;

Console.WriteLine("\nПісля відписки:");

var afterUnsubscribe = service.CreateOrder(
    "L6-" + Guid.NewGuid().ToString("N")[..8],
    "Лютик",
    "Оксенфурт"
);

Console.WriteLine(
    $"Створено замовлення: {afterUnsubscribe.Id}"
);

Console.WriteLine($"Подій отримано: {changes}");

Console.WriteLine("\nЗамовлення із сумою менше 1000:");

var filteredOrders = service.FindAll(
    o => o.Total < 1000m
);

foreach (var item in filteredOrders)
{
    Console.WriteLine(item);
}



//+2
string csvPath = Path.Combine(
    AppContext.BaseDirectory,
    "data",
    "orders-lab06.csv"
);

Directory.CreateDirectory(
    Path.GetDirectoryName(csvPath)!
);


File.WriteAllLines(csvPath, new[]
{
    "id;customer;address;price;comment",
    "O-601;Цірі;Каер Морхен;1500.00;Терміново",
    "O-602;;Новіград;100.00;Без імені",
    "O-603;Лютик;Оксенфурт;-50.00;Некоректна ціна"
});


var importResult = Core.Import.OrderCsvImporter.Load(
    csvPath,
    message => Console.WriteLine($"Пропущено: {message}")
);

Console.WriteLine(
    $"Успішно імпортовано: {importResult.Items.Count}"
);

Console.WriteLine(
    $"Помилок імпорту: {importResult.Errors.Count}"
);


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
