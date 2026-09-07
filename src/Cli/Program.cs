using System.Runtime.InteropServices;
Console.OutputEncoding = System.Text.Encoding.UTF8;

Console.WriteLine("DrackDrop");
Console.WriteLine("Студент: Сенів Артем, група ФЕІ-36");
Console.WriteLine(new string('-', 52));
Console.WriteLine($"ОС (OSDescription)  : {RuntimeInformation.OSDescription}");
Console.WriteLine($"ОС (Environment)    : {Environment.OSVersion}");
Console.WriteLine($"Архітектура процесу : {RuntimeInformation.ProcessArchitecture}");
Console.WriteLine($"Версія .NET (CLR)   : {Environment.Version}");
Console.WriteLine($"Runtime             : {RuntimeInformation.FrameworkDescription}");
Console.WriteLine($"Каталог застосунку  : {AppContext.BaseDirectory}");
Console.WriteLine($"Поточний каталог    : {Environment.CurrentDirectory}");
Console.WriteLine(new string('-', 52));
Console.WriteLine("Предметна область: Замовлення");
Console.WriteLine("Призначення: оформлення замовлень і підрахунок сум.");