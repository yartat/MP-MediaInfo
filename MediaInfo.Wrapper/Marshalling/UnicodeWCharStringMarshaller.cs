#if NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Runtime.CompilerServices;
using System.Text;

namespace MediaInfo.Marshalling;

[CustomMarshaller(typeof(string), MarshalMode.ManagedToUnmanagedIn, typeof(ManagedToUnmanagedIn))]
[CustomMarshaller(typeof(string), MarshalMode.ManagedToUnmanagedOut, typeof(ManagedToUnmanagedOut))]
internal static unsafe class UnicodeWCharStringMarshaller
{
  public ref struct ManagedToUnmanagedIn
  {
    public static int BufferSize => 256;

    private byte* _unmanagedValue;
    private bool _allocated; // Used stack alloc or allocated other memory

    public void FromManaged(string? managed, Span<byte> buffer)
    {
      if (managed is null)
      {
        _unmanagedValue = null;
        return;
      }

      if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
      {
        var num = checked(Encoding.Unicode.GetByteCount(managed) + 2);
        if (num >= buffer.Length)
        {
          _unmanagedValue = (byte*)Marshal.StringToCoTaskMemUni(managed);
          _allocated = true;
          return;
        }

        _unmanagedValue = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(buffer));
        var len = Encoding.Unicode.GetBytes(managed.AsSpan(), buffer);
        buffer[len] = 0;
        buffer[len + 1] = 0;
        _allocated = false;
        return;
      }

      {
        var num = checked(Encoding.UTF32.GetByteCount(managed) + 4);
        var allocated = false;
        if (num >= buffer.Length)
        {
          buffer = new Span<byte>(NativeMemory.Alloc((nuint)num), num);
          allocated = true;
        }

        _unmanagedValue = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(buffer));
        var len = Encoding.UTF32.GetBytes(managed.AsSpan(), buffer);
        buffer[len] = 0;
        buffer[len + 1] = 0;
        buffer[len + 2] = 0;
        buffer[len + 3] = 0;
        _allocated = allocated;
      }
    }

    public byte* ToUnmanaged() => _unmanagedValue;

    public void Free()
    {
      if (_allocated && _unmanagedValue is not null)
      {
        NativeMemory.Free(_unmanagedValue);
        _unmanagedValue = null;
        _allocated = false;
      }
    }
  }

  public ref struct ManagedToUnmanagedOut
  {
    private string? _result;

    public void FromUnmanaged(byte* unmanaged)
    {
      if (unmanaged is null)
      {
        return;
      }

      if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
      {
        _result = Marshal.PtrToStringUni((IntPtr)unmanaged);
        return;
      }

      // Scan for 4-byte (UTF-32) null terminator
      byte* ptr = unmanaged;
      while (*(uint*)ptr != 0)
      {
        ptr += 4;
      }
      var length = (int)(ptr - unmanaged);
      if (length == 0)
      {
        _result = string.Empty;
        return;
      }

      _result = Encoding.UTF32.GetString(unmanaged, length);
    }

    public string? ToManagedFinally() => _result;

    public void Free()
    {
    }
  }
}

#endif
