
namespace Core.Reports;

public static class LinqExtensions
{
    public static IEnumerable<T> TopBy<T>(
        this IEnumerable<T> source,
        Func<T, decimal> scoreSelector,
        int count,
        Func<T, string> tieBreaker)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(scoreSelector);
        ArgumentNullException.ThrowIfNull(tieBreaker);

        return source
            .OrderByDescending(scoreSelector)
            .ThenBy(tieBreaker, StringComparer.Ordinal)
            .Take(Math.Max(0, count));
    }
}
