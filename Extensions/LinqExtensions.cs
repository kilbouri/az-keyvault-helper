namespace KeyVaultHelper.Extensions;

public static class LinqExtensions
{
    public static IAsyncEnumerable<T> Flatten<T>(this IAsyncEnumerable<IEnumerable<T>> pageIter) => pageIter.SelectMany(static page => page);
    public static IAsyncEnumerable<T> Flatten<T>(this IAsyncEnumerable<IAsyncEnumerable<T>> pageIter) => pageIter.SelectMany(static page => page);
    public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> pageIter) => pageIter.SelectMany(static page => page);
}
