
using Core.Abstractions;
using Core.Events;

namespace Cli.Notifications;

public sealed class FileSink : INotificationSink
{
    private readonly string _path;

    public FileSink(string path)
    {
        _path = Path.GetFullPath(path);
    }

    public void Notify(OrderChangedEventArgs change)
    {
        Directory.CreateDirectory(
            Path.GetDirectoryName(_path)!
        );

        string line =
            $"{change.At:HH:mm:ss} {change.Kind} | " +
            $"ID: {change.OrderId} | " +
            $"Позицій: {change.LinesCount} | " +
            $"Сума: {change.Total:0.00}";

        File.AppendAllText(
            _path,
            line + Environment.NewLine
        );
    }
}
