
using System.Text;
using Core;
using Core.Domain;
using Core.Abstractions;
using Core.Storage;
using Core.Services;
using Core.Events;
using Cli.Notifications;
using Core.Reports;
Console.OutputEncoding = Encoding.UTF8;

string? reportCommand = args.FirstOrDefault(
    a => !a.StartsWith("--", StringComparison.Ordinal)
);

if (reportCommand is not null &&
    reportCommand.StartsWith("report-", StringComparison.Ordinal))
{
    bool reportUseFile = args.Contains("--file");

    string reportDataPath = Path.Combine(
        AppContext.BaseDirectory,
        "data",
        "orders.json"
    );

    IOrderStore reportStore = reportUseFile
        ? new FileOrderStore(reportDataPath)
        : new InMemoryOrderStore(SampleData.Orders());

    var items = reportStore.List();

    Console.WriteLine($"Сховище: {reportStore.GetType().Name}");
    Console.WriteLine($"Замовлень: {items.Count}");
    Console.WriteLine();

    switch (reportCommand)
    {
        case "report-top":
        {
            Console.WriteLine("ТОП-5 клієнтів за сумою:");

            var result = ReportService.TopCustomers(items, 5);

            foreach (var row in result)
            {
                Console.WriteLine(
                    $"{row.Customer,-22} " +
                    $"замовлень: {row.OrdersCount,2} | " +
                    $"сума: {row.TotalAmount,10:F2}"
                );
            }

            break;
        }

        case "report-high":
        {
            const decimal threshold = 2000m;

            Console.WriteLine(
                $"Замовлення дорожче {threshold:F2}:"
            );

            var result = ReportService.ExpensiveOrders(
                items,
                threshold
            );

            foreach (var row in result)
            {
                Console.WriteLine(
                    $"{row.Id,-12} " +
                    $"{row.Customer,-22} " +
                    $"{row.Total,10:F2} | " +
                    $"Підтверджено: {row.IsConfirmed}"
                );
            }

            Console.WriteLine(
                $"Знайдено: {result.Count}"
            );

            Console.WriteLine(
                "Порожнє джерело: " +
                ReportService.ExpensiveOrders(
                    Array.Empty<Order>(),
                    threshold
                ).Count
            );

            break;
        }

        case "report-groups":
        {
            Console.WriteLine("Групування за статусом:");

            var result = ReportService.ByStatus(items);

            foreach (var group in result)
            {
                Console.WriteLine(
                    $"{group.Status,-20} " +
                    $"замовлень: {group.OrdersCount,3} | " +
                    $"сума: {group.TotalAmount,10:F2}"
                );
            }

            break;
        }

        case "report-sum":
        {
            var totals = ReportService.Totals(items);

            Console.WriteLine("Загальна статистика:");
            Console.WriteLine($"Замовлень: {totals.OrdersCount}");
            Console.WriteLine($"Загальна сума: {totals.TotalAmount:F2}");
            Console.WriteLine($"Середній чек: {totals.AverageAmount:F2}");
            Console.WriteLine($"Максимальне замовлення: {totals.MaxAmount:F2}");

            var empty = ReportService.Totals(
                Array.Empty<Order>()
            );

            Console.WriteLine(
                $"Порожнє джерело: " +
                $"кількість={empty.OrdersCount}, " +
                $"середнє={empty.AverageAmount:F2}, " +
                $"максимум={empty.MaxAmount:F2}"
            );

            break;
        }

        case "report-deferred":
        {
            var demo = ReportService.DeferredDemo(items);

            Console.WriteLine("Демонстрація відкладеного виконання:");
            Console.WriteLine($"До додавання: {demo.Before}");
            Console.WriteLine($"Після додавання: {demo.After}");
            Console.WriteLine(
                $"Знімок після Clear: {demo.SnapshotAfterClear}"
            );
            Console.WriteLine(
                $"Запит після Clear: {demo.QueryAfterClear}"
            );

            break;
        }
         case "report-join":
{
    Console.WriteLine("GroupJoin: клієнти та їхні замовлення");

    var customers = new[]
    {
        "Геральт з Рівії",
        "Лютик",
        "Йеннефер",
        "Клієнт 4",
        "Клієнт без замовлень"
    };

    var result = ReportService.CustomersWithOrders(
        customers,
        items
    );

    foreach (var row in result)
    {
        Console.WriteLine(
            $"{row.Customer,-24} | " +
            $"замовлень: {row.OrdersCount,3} | " +
            $"сума: {row.TotalAmount,10:F2}"
        );
    }

    break;
}

case "report-top-orders":
{
    Console.WriteLine("ТОП-5 замовлень за сумою:");

    var result = ReportService.TopOrders(items, 5);

    foreach (var row in result)
    {
        Console.WriteLine(
            $"{row.Id,-12} | " +
            $"{row.Customer,-22} | " +
            $"{row.Total,10:F2}"
        );
    }

    break;
}

case "report-query":
{
    Console.WriteLine("LINQ Query Syntax:");

    var result = ReportService.ByStatusQuerySyntax(items);

    foreach (var row in result)
    {
        Console.WriteLine(
            $"{row.Status,-20} | " +
            $"замовлень: {row.OrdersCount,3} | " +
            $"сума: {row.TotalAmount,10:F2}"
        );
    }

    var methodResult = ReportService.ByStatus(items);

    Console.WriteLine(
        "Результати збігаються: " +
        result.SequenceEqual(methodResult)
    );

    break;
}

case "report-bench":
{
    Console.WriteLine("Порівняння LINQ на 100 000 записів:");

    var result = ReportBenchmarks.Compare(100_000);

    Console.WriteLine($"Кількість: {result.Records}");
    Console.WriteLine($"Контрольна сума: {result.Sum:F2}");

    Console.WriteLine(
        $"Без проміжного ToList: " +
        $"{result.WithoutListMs:F3} мс"
    );

    Console.WriteLine(
        $"Із проміжним ToList: " +
        $"{result.WithListMs:F3} мс"
    );

    break;
}
        default:
            Console.WriteLine(
                "Доступні команди: report-top | report-high | " +
                "report-groups | report-sum | report-deferred"
            );
            break;
    }

    return;
}

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

//+
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
