#region Copyright (c) MediaArea.net SARL.

/*  Copyright (c) MediaArea.net SARL. All Rights Reserved.
*
*  Use of this source code is governed by a BSD-style license that can
*  be found in the License.html file in the root of the source tree.
*/

//+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
//
// Microsoft Visual C# wrapper for MediaInfo Library
// See MediaInfo.h for help
//
// To make it working, you must put MediaInfo.Dll
// in the executable folder
//
//+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
#endregion

using System;
using System.Runtime.InteropServices;

#pragma warning disable 1591 // Disable XML documentation warnings

namespace MediaInfo
{
  public enum InfoFileOptions
  {
    Nothing = 0x00,
    NoRecursive = 0x01,
    CloseAll = 0x02,
    Max = 0x04
  };

  public enum StreamKind
  {
    General,
    Video,
    Audio,
    Text,
    Other,
    Image,
    Menu,
  }

  public enum InfoKind
  {
    Name,
    Text,
    Measure,
    Options,
    NameText,
    MeasureText,
    Info,
    HowTo
  }

  public enum InfoOptions
  {
    ShowInInform,
    Support,
    ShowInSupported,
    TypeOfValue
  }

  public class MediaInfo : IDisposable
  {
    private IntPtr _handle;
    private readonly bool _mustUseAnsi;

    //MediaInfo class
    public MediaInfo()
    {
      try
      {
        _handle = NativeMethods.MediaInfo_New();
      }
      catch
      {
        _handle = IntPtr.Zero;
      }

      _mustUseAnsi = Environment.OSVersion.ToString().IndexOf("Windows", StringComparison.OrdinalIgnoreCase) == -1;
    }

    ~MediaInfo()
    {
      Dispose(false);
    }

    public IntPtr Open(string fileName)
    {
      if (_handle == IntPtr.Zero)
      {
        return IntPtr.Zero;
      }

      if (_mustUseAnsi)
      {
        IntPtr result;
        using (var fileNameMemory = GlobalMemory.StringToGlobalAnsi(fileName))
        {
          result = NativeMethods.MediaInfoA_Open(_handle, fileNameMemory.Handle);
        }

        return result;
      }
      
      return NativeMethods.MediaInfo_Open(_handle, fileName);
    }

    public IntPtr OpenBufferInit(long fileSize, long fileOffset)
    {
      return _handle == IntPtr.Zero ? IntPtr.Zero : NativeMethods.MediaInfo_Open_Buffer_Init(_handle, fileSize, fileOffset);
    }

    public IntPtr OpenBufferContinue(IntPtr buffer, IntPtr bufferSize)
    {
      return _handle == IntPtr.Zero ? IntPtr.Zero : NativeMethods.MediaInfo_Open_Buffer_Continue(_handle, buffer, bufferSize);
    }

    public long OpenBufferContinueGoToGet()
    {
      return _handle == IntPtr.Zero ? 0 : NativeMethods.MediaInfo_Open_Buffer_Continue_GoTo_Get(_handle);
    }

    public IntPtr OpenBufferFinalize()
    {
      return _handle == IntPtr.Zero ? IntPtr.Zero : NativeMethods.MediaInfo_Open_Buffer_Finalize(_handle);
    }

    public void Close()
    {
      if (_handle != IntPtr.Zero)
      {
        NativeMethods.MediaInfo_Delete(_handle);
        _handle = IntPtr.Zero;
      }
    }

    public string Inform()
    {
      if (_handle == IntPtr.Zero)
      {
        return "Unable to load MediaInfo library";
      }

      return _mustUseAnsi
               ? Marshal.PtrToStringAnsi(NativeMethods.MediaInfoA_Inform(_handle, IntPtr.Zero))
               : Marshal.PtrToStringUni(NativeMethods.MediaInfo_Inform(_handle, IntPtr.Zero));
    }

    public string Get(StreamKind streamKind, int streamNumber, string parameter, InfoKind kindOfInfo, InfoKind kindOfSearch)
    {
      if (_handle == IntPtr.Zero)
      {
        return "Unable to load MediaInfo library";
      }

      if (_mustUseAnsi)
      {
        string result;
        using (var parameterPtr = GlobalMemory.StringToGlobalAnsi(parameter))
        {
          result =
            Marshal.PtrToStringAnsi(
              NativeMethods.MediaInfoA_Get(
                _handle,
                (IntPtr)streamKind,
                (IntPtr)streamNumber,
                parameterPtr.Handle,
                (IntPtr)kindOfInfo,
                (IntPtr)kindOfSearch));
        }

        return result;
      }

      return
        Marshal.PtrToStringUni(
          NativeMethods.MediaInfo_Get(
            _handle,
            (IntPtr)streamKind,
            (IntPtr)streamNumber,
            parameter,
            (IntPtr)kindOfInfo,
            (IntPtr)kindOfSearch));
    }

    public string Get(StreamKind streamKind, int streamNumber, int parameter, InfoKind kindOfInfo)
    {
      if (_handle == IntPtr.Zero)
      {
        return "Unable to load MediaInfo library";
      }

      return _mustUseAnsi ? 
        Marshal.PtrToStringAnsi(NativeMethods.MediaInfoA_GetI(_handle, (IntPtr)streamKind, (IntPtr)streamNumber, (IntPtr)parameter, (IntPtr)kindOfInfo)) : 
        Marshal.PtrToStringUni(NativeMethods.MediaInfo_GetI(_handle, (IntPtr)streamKind, (IntPtr)streamNumber, (IntPtr)parameter, (IntPtr)kindOfInfo));
    }

    public string Option(string option, string value)
    {
      if (_handle == IntPtr.Zero)
      {
        return "Unable to load MediaInfo library";
      }

      if (_mustUseAnsi)
      {
        string result;
        using (var optionPtr = GlobalMemory.StringToGlobalAnsi(option))
        using (var valuePtr = GlobalMemory.StringToGlobalAnsi(value))
        {
          result = Marshal.PtrToStringAnsi(NativeMethods.MediaInfoA_Option(_handle, optionPtr.Handle, valuePtr.Handle));
        }

        return result;
      }
      
      return Marshal.PtrToStringUni(NativeMethods.MediaInfo_Option(_handle, option, value));
    }

    public IntPtr StateGet()
    {
      return _handle == IntPtr.Zero ? IntPtr.Zero : NativeMethods.MediaInfo_State_Get(_handle);
    }

    public int CountGet(StreamKind streamKind, int streamNumber)
    {
      return _handle == IntPtr.Zero ? 0 : (int)NativeMethods.MediaInfo_Count_Get(_handle, (IntPtr)streamKind, (IntPtr)streamNumber);
    }

    //Default values, if you know how to set default values in C#, say me
    public string Get(StreamKind streamKind, int streamNumber, string parameter, InfoKind kindOfInfo)
    {
      return Get(streamKind, streamNumber, parameter, kindOfInfo, InfoKind.Name);
    }

    public string Get(StreamKind streamKind, int streamNumber, string parameter)
    {
      return Get(streamKind, streamNumber, parameter, InfoKind.Text, InfoKind.Name);
    }

    public string Get(StreamKind streamKind, int streamNumber, int parameter)
    {
      return Get(streamKind, streamNumber, parameter, InfoKind.Text);
    }

    public string Option(string option)
    {
      return Option(option, string.Empty);
    }

    public int CountGet(StreamKind streamKind)
    {
      return CountGet(streamKind, -1);
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    // ReSharper disable once UnusedParameter.Local
    private void Dispose(bool disposing)
    {
      Close();
    }
  }

  public class MediaInfoList : IDisposable
  {
    private IntPtr _handle;

    //MediaInfo class
    public MediaInfoList()
    {
      _handle = NativeMethods.MediaInfoList_New();
    }

    ~MediaInfoList()
    {
      Dispose(false);
    }

    public int Open(string fileName, InfoFileOptions options)
    {
      return (int)NativeMethods.MediaInfoList_Open(_handle, fileName, (IntPtr)options);
    }

    public void Close(int filePos)
    {
      NativeMethods.MediaInfoList_Close(_handle, (IntPtr)filePos);
    }

    public string Inform(int filePos)
    {
      return Marshal.PtrToStringUni(NativeMethods.MediaInfoList_Inform(_handle, (IntPtr)filePos, (IntPtr)0));
    }

    public string Get(int filePos, StreamKind streamKind, int streamNumber, string parameter, InfoKind kindOfInfo, InfoKind kindOfSearch)
    {
      return Marshal.PtrToStringUni(NativeMethods.MediaInfoList_Get(_handle, (IntPtr)filePos, (IntPtr)streamKind, (IntPtr)streamNumber, parameter, (IntPtr)kindOfInfo, (IntPtr)kindOfSearch));
    }

    public string Get(int filePos, StreamKind streamKind, int streamNumber, int parameter, InfoKind kindOfInfo)
    {
      return Marshal.PtrToStringUni(NativeMethods.MediaInfoList_GetI(_handle, (IntPtr)filePos, (IntPtr)streamKind, (IntPtr)streamNumber, (IntPtr)parameter, (IntPtr)kindOfInfo));
    }

    public string Option(string option, string value)
    {
      return Marshal.PtrToStringUni(NativeMethods.MediaInfoList_Option(_handle, option, value));
    }

    public int StateGet()
    {
      return (int)NativeMethods.MediaInfoList_State_Get(_handle);
    }

    public int CountGet(int filePos, StreamKind streamKind, int streamNumber)
    {
      return (int)NativeMethods.MediaInfoList_Count_Get(_handle, (IntPtr)filePos, (IntPtr)streamKind, (IntPtr)streamNumber);
    }

    //Default values, if you know how to set default values in C#, say me
    public void Open(string fileName)
    {
      Open(fileName, 0);
    }

    public void Close()
    {
      Close(-1);
    }

    public string Get(int filePos, StreamKind streamKind, int streamNumber, string parameter, InfoKind kindOfInfo)
    {
      return Get(filePos, streamKind, streamNumber, parameter, kindOfInfo, InfoKind.Name);
    }

    public string Get(int filePos, StreamKind streamKind, int streamNumber, string parameter)
    {
      return Get(filePos, streamKind, streamNumber, parameter, InfoKind.Text, InfoKind.Name);
    }

    public string Get(int filePos, StreamKind streamKind, int streamNumber, int parameter)
    {
      return Get(filePos, streamKind, streamNumber, parameter, InfoKind.Text);
    }

    public string Option(string option)
    {
      return Option(option, string.Empty);
    }

    public int CountGet(int filePos, StreamKind streamKind)
    {
      return CountGet(filePos, streamKind, -1);
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    // ReSharper disable once UnusedParameter.Local
    private void Dispose(bool disposing)
    {
      if (_handle != IntPtr.Zero)
      {
        NativeMethods.MediaInfoList_Delete(_handle);
        _handle = IntPtr.Zero;
      }
    }
  }
}