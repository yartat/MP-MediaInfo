#if NET5_0_OR_GREATER
#else
namespace System.Runtime.CompilerServices;

/// <summary>
/// Lets a record declare init only properties on a target framework whose runtime does not define this type.
/// </summary>
internal static class IsExternalInit
{
}

#endif
