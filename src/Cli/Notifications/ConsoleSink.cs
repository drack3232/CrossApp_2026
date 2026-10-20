
using Core.Abstractions;
using Core.Events;

namespace Cli.Notifications;

public sealed class ConsoleSink : INotificationSink
{
    public void Notify(OrderChangedEventArgs change)
    {
        Console.WriteLine(
            $"[ConsoleSink] {change.Kind} | " +
            $"ID: {change.OrderId} | " +
            $"Позицій: {change.LinesCount} | " +
            $"Сума: {change.Total:0.00}"
        );
    }
}
