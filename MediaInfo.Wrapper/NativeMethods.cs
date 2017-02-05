#region Copyright (C) 2005-2017 Team MediaPortal

// Copyright (C) 2005-2017 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Runtime.InteropServices;

namespace MediaInfo
{
  internal static class NativeMethods
  {
    internal enum Status
    {
      None = 0x00,
      Accepted = 0x01,
      Filled = 0x02,
      Updated = 0x04,
      Finalized = 0x08,
    }

    [DllImport("kernel32.dll")]
    internal static extern long GetDriveType(string driveLetter);

    //Import of DLL functions. DO NOT USE until you know what you do (MediaInfo DLL do NOT use CoTaskMemAlloc to allocate memory)
    [DllImport("MediaInfo.dll")]
    internal static extern IntPtr MediaInfo_New();

    [DllImport("MediaInfo.dll")]
    internal static extern void MediaInfo_Delete(IntPtr handle);

    [DllImport("MediaInfo.dll")]
    internal static extern IntPtr MediaInfo_Open(IntPtr handle, [MarshalAs(UnmanagedType.LPWStr)] string fileName);

    [DllImport("MediaInfo.dll")]
    internal static extern IntPtr MediaInfoA_Open(IntPtr handle, IntPtr fileName);

    [DllImport("MediaInfo.dll")]
    internal static extern IntPtr MediaInfo_Open_Buffer_Init(IntPtr handle, long fileSize, long fileOffset);

    [DllImport("MediaInfo.dll")]
    internal static extern IntPtr MediaInfoA_Open(IntPtr handle, long fileSize, long fileOffset);

    [DllImport("MediaInfo.dll")]
    internal static extern IntPtr MediaInfo_Open_Buffer_Continue(IntPtr handle, IntPtr buffer, IntPtr bufferSize);

    [DllImport("MediaInfo.dll")]
    internal static extern IntPtr MediaInfoA_Open_Buffer_Continue(
      IntPtr handle,
      long fileSize,
      byte[] buffer,
      IntPtr bufferSize);

    [DllImport("MediaInfo.dll")]
    internal static extern long MediaInfo_Open_Buffer_Continue_GoTo_Get(IntPtr handle);

    [DllImport("MediaInfo.dll")]
    internal static extern long MediaInfoA_Open_Buffer_Continue_GoTo_Get(IntPtr handle);

    [DllImport("MediaInfo.dll")]
    internal static extern IntPtr MediaInfo_Open_Buffer_Finalize(IntPtr handle);

    [DllImport("MediaInfo.dll")]
    internal static extern IntPtr MediaInfoA_Open_Buffer_Finalize(IntPtr handle);

    [DllImport("MediaInfo.dll")]
    internal static extern void MediaInfo_Close(IntPtr handle);

    [DllImport("MediaInfo.dll")]
    internal static extern IntPtr MediaInfo_Inform(IntPtr handle, IntPtr reserved);

    [DllImport("MediaInfo.dll")]
    internal static extern IntPtr MediaInfoA_Inform(IntPtr handle, IntPtr reserved);

    [DllImport("MediaInfo.dll")]
    internal static extern IntPtr MediaInfo_GetI(
      IntPtr handle,
      IntPtr streamKind,
      IntPtr streamNumber,
      IntPtr parameter,
      IntPtr kindOfInfo);

    [DllImport("MediaInfo.dll")]
    internal static extern IntPtr MediaInfoA_GetI(
      IntPtr handle,
      IntPtr streamKind,
      IntPtr streamNumber,
      IntPtr parameter,
      IntPtr kindOfInfo);

    [DllImport("MediaInfo.dll")]
    internal static extern IntPtr MediaInfo_Get(
      IntPtr handle,
      IntPtr streamKind,
      IntPtr streamNumber,
      [MarshalAs(UnmanagedType.LPWStr)] string parameter,
      IntPtr kindOfInfo,
      IntPtr kindOfSearch);

    [DllImport("MediaInfo.dll")]
    internal static extern IntPtr MediaInfoA_Get(
      IntPtr handle,
      IntPtr streamKind,
      IntPtr streamNumber,
      IntPtr parameter,
      IntPtr kindOfInfo,
      IntPtr kindOfSearch);

    [DllImport("MediaInfo.dll")]
    internal static extern IntPtr MediaInfo_Option(
      IntPtr handle,
      [MarshalAs(UnmanagedType.LPWStr)] string option,
      [MarshalAs(UnmanagedType.LPWStr)] string value);

    [DllImport("MediaInfo.dll")]
    internal static extern IntPtr MediaInfoA_Option(IntPtr handle, IntPtr option, IntPtr value);

    [DllImport("MediaInfo.dll")]
    internal static extern IntPtr MediaInfo_State_Get(IntPtr handle);

    [DllImport("MediaInfo.dll")]
    internal static extern IntPtr MediaInfo_Count_Get(IntPtr handle, IntPtr streamKind, IntPtr streamNumber);

    [DllImport("MediaInfo.dll")]
    internal static extern IntPtr MediaInfoList_New();

    [DllImport("MediaInfo.dll")]
    internal static extern void MediaInfoList_Delete(IntPtr handle);

    [DllImport("MediaInfo.dll")]
    internal static extern IntPtr MediaInfoList_Open(
      IntPtr handle,
      [MarshalAs(UnmanagedType.LPWStr)] string fileName,
      IntPtr options);

    [DllImport("MediaInfo.dll")]
    internal static extern void MediaInfoList_Close(IntPtr handle, IntPtr filePos);

    [DllImport("MediaInfo.dll")]
    internal static extern IntPtr MediaInfoList_Inform(IntPtr handle, IntPtr filePos, IntPtr reserved);

    [DllImport("MediaInfo.dll")]
    internal static extern IntPtr MediaInfoList_GetI(
      IntPtr handle,
      IntPtr filePos,
      IntPtr streamKind,
      IntPtr streamNumber,
      IntPtr parameter,
      IntPtr kindOfInfo);

    [DllImport("MediaInfo.dll")]
    internal static extern IntPtr MediaInfoList_Get(
      IntPtr handle,
      IntPtr filePos,
      IntPtr streamKind,
      IntPtr streamNumber,
      [MarshalAs(UnmanagedType.LPWStr)] string parameter,
      IntPtr kindOfInfo,
      IntPtr kindOfSearch);

    [DllImport("MediaInfo.dll")]
    internal static extern IntPtr MediaInfoList_Option(
      IntPtr handle,
      [MarshalAs(UnmanagedType.LPWStr)] string option,
      [MarshalAs(UnmanagedType.LPWStr)] string value);

    [DllImport("MediaInfo.dll")]
    internal static extern IntPtr MediaInfoList_State_Get(IntPtr handle);

    [DllImport("MediaInfo.dll")]
    internal static extern IntPtr MediaInfoList_Count_Get(
      IntPtr handle,
      IntPtr filePos,
      IntPtr streamKind,
      IntPtr streamNumber);
  }
}