using Core;
using Core.Dto;
using Core.Import;
Console.OutputEncoding = System.Text.Encoding.UTF8;

EnvironmentReport report = EnvironmentInfo.Collect();

Console.WriteLine("DrackDrop");
Console.WriteLine("Студент: Сенів Артем, група ФЕІ-36");
Console.WriteLine(new string('-', 52));
Console.WriteLine($"ОС              : {report.OsDescription}");
Console.WriteLine($"Runtime         : {report.FrameworkDescription}");
Console.WriteLine($"Архітектура     : {report.ProcessArchitecture}");
Console.WriteLine($"RID (визначено) : {report.DetectedRid}");
Console.WriteLine($"RID (від .NET)  : {report.ReportedRid}");
Console.WriteLine($"Каталог         : {report.BaseDirectory}");
Console.WriteLine($"Нотатка збірки  : {report.BuildNote}");
Console.WriteLine(new string('-', 52));
Console.WriteLine("Предметна область: Замовлення (Customer, Product, Order, OrderLine)");

string path = args.Length > 0 ? args[0] : Path.Combine("data", "sample.csv");

if (!File.Exists(path))
{
    Console.WriteLine($"Файл не знайдено: {Path.GetFullPath(path)}");
    return 1;
}

ImportResult<OrderDto> result = OrderCsvImporter.Load(path);


Console.WriteLine($"Завантажено записів: {result.Items.Count}");
foreach (OrderDto o in result.Items.Take(10))
{
    Console.WriteLine($"{o.Id,-6} {o.Customer,-20} {o.Price,8} {o.Address}");
}


if (result.Errors.Count > 0)
{
    Console.WriteLine($"\nПропущено рядків: {result.Errors.Count}");
    foreach (string e in result.Errors)
    {
        Console.WriteLine($" ! {e}");
    }
}

return 0;