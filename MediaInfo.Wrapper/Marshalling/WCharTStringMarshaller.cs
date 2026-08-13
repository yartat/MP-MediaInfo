#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

#if NETSTANDARD2_0_OR_GREATER || NETCOREAPP || NET5_0_OR_GREATER

using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Text;

namespace MediaInfo.Marshalling
{
  /// <summary>
  /// Custom marshaller that converts .NET <see cref="string"/> instances to and from
  /// native <c>wchar_t*</c> strings for P/Invoke calls.
  /// <list type="bullet">
  ///   <item><description>Windows – <c>wchar_t</c> is 2 bytes (UTF-16 LE).</description></item>
  ///   <item><description>Unix / macOS – <c>wchar_t</c> is 4 bytes (UTF-32 LE).</description></item>
  /// </list>
  /// Use this marshaller with <see cref="MarshalAsAttribute"/> on <c>[DllImport]</c> string
  /// parameters:
  /// <code>
  /// [MarshalAs(UnmanagedType.CustomMarshaler,
  ///            MarshalTypeRef = typeof(WCharTStringMarshaller))]
  /// string parameter
  /// </code>
  /// </summary>
  /// <remarks>
  /// Memory allocated by <see cref="MarshalManagedToNative"/> is freed in
  /// <see cref="CleanUpNativeData"/> via <see cref="Marshal.FreeHGlobal"/>.
  /// Native pointers returned directly by the library must <em>not</em> be freed here;
  /// those should remain as <see cref="IntPtr"/> return values and be converted manually.
  /// </remarks>
  public sealed class WCharTStringMarshaller : ICustomMarshaler
  {
    private static readonly WCharTStringMarshaller _instance = new();
    private static ConcurrentDictionary<IntPtr, bool> _allocatedPointers = new();

    /// <summary>
    /// Returns the singleton marshaller instance required by the P/Invoke infrastructure.
    /// </summary>
    /// <param name="cookie">Unused marshaller cookie.</param>
    /// <returns>The shared <see cref="WCharTStringMarshaller"/> instance.</returns>
    public static ICustomMarshaler GetInstance(string cookie) => _instance;

    /// <inheritdoc/>
    public IntPtr MarshalManagedToNative(object managedObj)
    {
      if (managedObj is null)
      {
        return IntPtr.Zero;
      }

      if (managedObj is not string str)
      {
        throw new MarshalDirectiveException(
          $"{nameof(WCharTStringMarshaller)} can only be used on {nameof(String)} parameters.");
      }

      if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
      {
        // UTF-16 LE: 2 bytes per character + 2-byte null terminator
        var ptr = Marshal.StringToCoTaskMemUni(str);
        _allocatedPointers.TryAdd(ptr, true);
        return ptr;
      }
      else
      {
        // UTF-32 LE: 4 bytes per character + 4-byte null terminator
        var bytes = Encoding.UTF32.GetBytes(str);
        var ptr = Marshal.AllocCoTaskMem(bytes.Length + 4);
        Marshal.Copy(bytes, 0, ptr, bytes.Length);
        Marshal.WriteInt32(ptr, bytes.Length, 0);
        _allocatedPointers.TryAdd(ptr, true);
        return ptr;
      }
    }

    /// <inheritdoc/>
    public object MarshalNativeToManaged(IntPtr pNativeData)
    {
      if (pNativeData == IntPtr.Zero)
      {
        return null!;
      }

      if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
      {
        // Read null-terminated UTF-16 string
        return Marshal.PtrToStringUni(pNativeData)!;
      }

      // Scan for 4-byte (UTF-32) null terminator
      int charCount = 0;
      while (Marshal.ReadInt32(pNativeData, charCount * 4) != 0)
      {
        charCount++;
      }

      if (charCount == 0)
      {
        return string.Empty;
      }

      var bytes = new byte[charCount * 4];
      Marshal.Copy(pNativeData, bytes, 0, bytes.Length);
      return Encoding.UTF32.GetString(bytes);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Only frees memory that was allocated by <see cref="MarshalManagedToNative"/>.
    /// Do not apply this marshaller to parameters where the native side owns the buffer.
    /// </remarks>
    public void CleanUpNativeData(IntPtr pNativeData)
    {
      if (pNativeData != IntPtr.Zero && _allocatedPointers.TryRemove(pNativeData, out _))
      {
        Marshal.FreeCoTaskMem(pNativeData);
      }
    }

    /// <inheritdoc/>
    public void CleanUpManagedData(object managedObj) { }

    /// <inheritdoc/>
    /// <returns>-1, indicating a variable-length type.</returns>
    public int GetNativeDataSize() => -1;
  }
}

#endif
