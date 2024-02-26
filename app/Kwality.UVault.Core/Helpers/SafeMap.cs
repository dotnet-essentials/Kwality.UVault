namespace Kwality.UVault.Core.Helpers;

using global::System.Diagnostics.CodeAnalysis;
using global::System.Runtime.CompilerServices;

public static class SafeMap
{
    [SuppressMessage("ReSharper", "NullableWarningSuppressionIsUsed")]
    public static TDestination UnsafeAs<TSource, TDestination>(this TSource source)
        where TDestination : class
    {
        return Unsafe.As<TDestination>(source)!;
    }
}
