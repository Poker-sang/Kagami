namespace Kagami.Extensions;

internal static class WeakReferenceExtensions
{
    public static T Get<T>(this WeakReference<T?> target, Func<T> @default) where T : class
    {
        if (!target.TryGetTarget(out var value))
            target.SetTarget(value = @default());
        return value;
    }
}
