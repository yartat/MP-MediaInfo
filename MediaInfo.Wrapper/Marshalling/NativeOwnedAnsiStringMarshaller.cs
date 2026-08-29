#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.Runtime.InteropServices;
#if NET7_0_OR_GREATER
using System.Runtime.InteropServices.Marshalling;
#endif

namespace MediaInfo.Marshalling
{
#if NET7_0_OR_GREATER

  /// <summary>
  /// Reads a <c>char*</c> string returned by the library without taking ownership of it.
  /// </summary>
  /// <remarks>
  /// The library keeps owning the buffer it returns and does not allocate it with <c>CoTaskMemAlloc</c>. Marshalling
  /// such a return value as <see cref="UnmanagedType.LPStr"/> makes the runtime free it with <c>CoTaskMemFree</c>,
  /// which corrupts the process heap. This marshaller therefore only copies the characters out, and deliberately
  /// declares no <c>Free</c> method so that nothing can release the buffer.
  /// <para>
  /// It marshals in one direction only. Never apply it to a parameter, where the buffer would leak instead.
  /// </para>
  /// </remarks>
  [CustomMarshaller(typeof(string), MarshalMode.ManagedToUnmanagedOut, typeof(NativeOwnedAnsiStringMarshaller))]
  internal static unsafe class NativeOwnedAnsiStringMarshaller
  {
    /// <summary>
    /// Copies the string the library returned.
    /// </summary>
    /// <param name="unmanaged">The pointer the library returned, which stays owned by the library.</param>
    /// <returns>Returns the string, or <see cref="string.Empty"/> when the pointer is null.</returns>
    public static string ConvertToManaged(byte* unmanaged) =>
      unmanaged is null ? string.Empty : Marshal.PtrToStringAnsi((IntPtr)unmanaged) ?? string.Empty;
  }

#else

  /// <summary>
  /// Reads a <c>char*</c> string returned by the library without taking ownership of it.
  /// </summary>
  /// <remarks>
  /// The library keeps owning the buffer it returns and does not allocate it with <c>CoTaskMemAlloc</c>. Marshalling
  /// such a return value as <see cref="UnmanagedType.LPStr"/> makes the runtime free it with <c>CoTaskMemFree</c>,
  /// which corrupts the process heap. <see cref="CleanUpNativeData"/> is therefore empty by design.
  /// <para>
  /// Apply it to a return value only:
  /// <code>
  /// [return: MarshalAs(UnmanagedType.CustomMarshaler,
  ///                    MarshalTypeRef = typeof(NativeOwnedAnsiStringMarshaller))]
  /// </code>
  /// Applying it to a parameter throws, because this marshaller cannot produce a native buffer.
  /// </para>
  /// </remarks>
  public sealed class NativeOwnedAnsiStringMarshaller : ICustomMarshaler
  {
    private static readonly NativeOwnedAnsiStringMarshaller Instance = new();

    /// <summary>
    /// Returns the shared marshaller instance the platform invoke infrastructure asks for.
    /// </summary>
    /// <param name="cookie">The unused marshaller cookie.</param>
    /// <returns>Returns the shared instance.</returns>
    public static ICustomMarshaler GetInstance(string cookie) => Instance;

    /// <inheritdoc />
    public object MarshalNativeToManaged(IntPtr pNativeData) =>
      pNativeData == IntPtr.Zero ? string.Empty : Marshal.PtrToStringAnsi(pNativeData) ?? string.Empty;

    /// <inheritdoc />
    /// <exception cref="MarshalDirectiveException">Always, because this marshaller reads return values only.</exception>
    public IntPtr MarshalManagedToNative(object managedObj) =>
      throw new MarshalDirectiveException(
        $"{nameof(NativeOwnedAnsiStringMarshaller)} marshals return values only, because it never owns the buffer " +
        "it reads. Marshal a parameter with UnmanagedType.LPStr or WCharTStringMarshaller instead.");

    /// <inheritdoc />
    /// <remarks>Empty by design: the buffer belongs to the library and freeing it corrupts the heap.</remarks>
    public void CleanUpNativeData(IntPtr pNativeData)
    {
    }

    /// <inheritdoc />
    public void CleanUpManagedData(object managedObj)
    {
    }

    /// <inheritdoc />
    /// <returns>Returns -1, which marks the type as variable length.</returns>
    public int GetNativeDataSize() => -1;
  }

#endif
}
