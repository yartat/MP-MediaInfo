#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

#if !NETFRAMEWORK

using System;
using System.Runtime.InteropServices;
using System.Text;
#if NET7_0_OR_GREATER
using System.Runtime.InteropServices.Marshalling;
#endif

namespace MediaInfo.Marshalling
{
#if NET7_0_OR_GREATER

  /// <summary>
  /// Reads a <c>wchar_t*</c> string returned by the library without taking ownership of it.
  /// </summary>
  /// <remarks>
  /// The width of <c>wchar_t</c> depends on the toolchain the library was built with:
  /// <list type="bullet">
  ///   <item><description>Windows and macOS — two bytes, UTF-16 LE.</description></item>
  ///   <item><description>Linux, where the library is built with GCC — four bytes, UTF-32 LE.</description></item>
  /// </list>
  /// <para>
  /// The library keeps owning the buffer it returns and does not allocate it with <c>CoTaskMemAlloc</c>. Marshalling
  /// such a return value as a string makes the runtime free it, which corrupts the process heap. This marshaller
  /// therefore only copies the characters out, and deliberately declares no <c>Free</c> method so that nothing can
  /// release the buffer. That is what separates it from <see cref="UnicodeWCharStringMarshaller"/>, which owns the
  /// buffers it creates for parameters and frees them.
  /// </para>
  /// </remarks>
  [CustomMarshaller(typeof(string), MarshalMode.ManagedToUnmanagedOut, typeof(NativeOwnedWCharTStringMarshaller))]
  internal static unsafe class NativeOwnedWCharTStringMarshaller
  {
    /// <summary>
    /// Copies the string the library returned.
    /// </summary>
    /// <param name="unmanaged">The pointer the library returned, which stays owned by the library.</param>
    /// <returns>Returns the string, or <see cref="string.Empty"/> when the pointer is null.</returns>
    public static string ConvertToManaged(byte* unmanaged)
    {
      if (unmanaged is null)
      {
        return string.Empty;
      }

      if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
      {
        return Marshal.PtrToStringUni((IntPtr)unmanaged) ?? string.Empty;
      }

      // Four byte characters, so the terminator is a zero int rather than a zero short.
      var end = unmanaged;
      while (*(uint*)end != 0)
      {
        end += sizeof(uint);
      }

      var length = (int)(end - unmanaged);
      return length == 0 ? string.Empty : Encoding.UTF32.GetString(unmanaged, length);
    }
  }

#else

  /// <summary>
  /// Reads a <c>wchar_t*</c> string returned by the library without taking ownership of it.
  /// </summary>
  /// <remarks>
  /// The width of <c>wchar_t</c> depends on the toolchain the library was built with:
  /// <list type="bullet">
  ///   <item><description>Windows and macOS — two bytes, UTF-16 LE.</description></item>
  ///   <item><description>Linux, where the library is built with GCC — four bytes, UTF-32 LE.</description></item>
  /// </list>
  /// <para>
  /// The library keeps owning the buffer it returns and does not allocate it with <c>CoTaskMemAlloc</c>. Marshalling
  /// such a return value as a string makes the runtime free it, which corrupts the process heap.
  /// <see cref="CleanUpNativeData"/> is therefore empty by design. That is what separates this marshaller from
  /// <see cref="WCharTStringMarshaller"/>, which owns the buffers it creates for parameters and frees them.
  /// </para>
  /// <para>
  /// Apply it to a return value only:
  /// <code>
  /// [return: MarshalAs(UnmanagedType.CustomMarshaler,
  ///                    MarshalTypeRef = typeof(NativeOwnedWCharTStringMarshaller))]
  /// </code>
  /// Applying it to a parameter throws, because this marshaller cannot produce a native buffer.
  /// </para>
  /// </remarks>
  public sealed class NativeOwnedWCharTStringMarshaller : ICustomMarshaler
  {
    private static readonly NativeOwnedWCharTStringMarshaller Instance = new();

    /// <summary>
    /// Returns the shared marshaller instance the platform invoke infrastructure asks for.
    /// </summary>
    /// <param name="cookie">The unused marshaller cookie.</param>
    /// <returns>Returns the shared instance.</returns>
    public static ICustomMarshaler GetInstance(string cookie) => Instance;

    /// <inheritdoc />
    public object MarshalNativeToManaged(IntPtr pNativeData)
    {
      if (pNativeData == IntPtr.Zero)
      {
        return string.Empty;
      }

      if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
      {
        return Marshal.PtrToStringUni(pNativeData) ?? string.Empty;
      }

      // Four byte characters, so the terminator is a zero int rather than a zero short.
      var characters = 0;
      while (Marshal.ReadInt32(pNativeData, characters * sizeof(int)) != 0)
      {
        characters++;
      }

      if (characters == 0)
      {
        return string.Empty;
      }

      var bytes = new byte[characters * sizeof(int)];
      Marshal.Copy(pNativeData, bytes, 0, bytes.Length);
      return Encoding.UTF32.GetString(bytes);
    }

    /// <inheritdoc />
    /// <exception cref="MarshalDirectiveException">Always, because this marshaller reads return values only.</exception>
    public IntPtr MarshalManagedToNative(object managedObj) =>
      throw new MarshalDirectiveException(
        $"{nameof(NativeOwnedWCharTStringMarshaller)} marshals return values only, because it never owns the " +
        $"buffer it reads. Marshal a parameter with {nameof(WCharTStringMarshaller)} instead.");

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

#endif
