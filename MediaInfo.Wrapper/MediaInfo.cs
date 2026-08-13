#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.IO;
using System.Runtime.InteropServices;

#pragma warning disable 1591 // Disable XML documentation warnings

namespace MediaInfo
{
  /// <summary>
  /// Describes options to open file in MediaInfoList
  /// </summary>
  [Flags]
  public enum InfoFileOptions
  {
    /// <summary>
    /// Default value, no options
    /// </summary>
    Nothing = 0x00,

    /// <summary>
    /// Do not process directories recursively
    /// </summary>
    NoRecursive = 0x01,

    /// <summary>
    /// Close all files
    /// </summary>
    CloseAll = 0x02,

    /// <summary>
    /// Maximum value, used for validation
    /// </summary>
    Max = 0x04
  };

  /// <summary>
  /// Describes stream kinds
  /// </summary>
  public enum StreamKind
  {
    /// <summary>
    /// The general (container, disk info)
    /// </summary>
    General,

    /// <summary>
    /// The video stream
    /// </summary>
    Video,

    /// <summary>
    /// The audio stream
    /// </summary>
    Audio,

    /// <summary>
    /// The subtitles and text stream
    /// </summary>
    Text,

    /// <summary>
    /// The other (chapters) stream kind
    /// </summary>
    Other,

    /// <summary>
    /// The image stream
    /// </summary>
    Image,

    /// <summary>
    /// The menu stream
    /// </summary>
    Menu,
  }

  /// <summary>
  /// Describes kinds of information
  /// </summary>
  public enum InfoKind
  {
    /// <summary>
    /// The name of the parameter
    /// </summary>
    Name,

    /// <summary>
    /// The text value of the parameter
    /// </summary>
    Text,

    /// <summary>
    /// The measure of the parameter
    /// </summary>
    Measure,

    /// <summary>
    /// The options of the parameter
    /// </summary>
    Options,

    /// <summary>
    /// The text of the name of the parameter
    /// </summary>
    NameText,

    /// <summary>
    /// The text of the measure of the parameter
    /// </summary>
    MeasureText,

    /// <summary>
    /// The information of the parameter
    /// </summary>
    Info,

    /// <summary>
    /// The how to of the parameter
    /// </summary>
    HowTo
  }

  /// <summary>
  /// Describes kinds of information to search in MediaInfoList
  /// </summary>
  public enum InfoOptions
  {
    /// <summary>
    /// The name of the parameter
    /// </summary>
    ShowInInform,

    /// <summary>
    /// The name of the parameter in case it is supported, else empty string
    /// </summary>
    Support,

    /// <summary>
    /// The name of the parameter in case it is supported, else the name of the parameter
    /// </summary>
    ShowInSupported,

    /// <summary>
    /// The value of the parameter in case it is supported, else empty string
    /// </summary>
    TypeOfValue
  }

  /// <summary>
  /// Describes low-level function to access to mediaInfo library
  /// </summary>
  /// <remarks>
  /// All functions in this class will return empty string if library is not loaded successfully. So, you can check if library is loaded by checking if Inform() method returns empty string or not.
  /// </remarks>
  /// <seealso cref="IDisposable" />
  public class MediaInfo : IDisposable
  {
#if NETFRAMEWORK
    private const string MediaInfoFileName = "MediaInfo.dll";
    private const string LibCurlFileName = "libcurl.dll";
    private const string LibCryptoFileName = "libcrypto-4.dll";
    private const string LibSslFileName = "libssl-4.dll";
    private const string LibCryptoFileName64Bit = "libcrypto-4-x64.dll";
    private const string LibSslFileName64Bit = "libssl-4-x64.dll";
    private const string LibCryptoFileNameArm64 = "libcrypto-4-arm64.dll";
    private const string LibSslFileNameArm64 = "libssl-4-arm64.dll";
    private const string LibSshFileName = "libssh2.dll";
    private const string BrotliCommonFileName = "brotlicommon.dll";
    private const string BrotliDecFileName = "brotlidec.dll";
    private const string BrotliEncFileName = "brotlienc.dll";
    private IntPtr _module;
#else
    private const string MediaInfoFileName = "libmediainfo";
    private const string LibCurlFileName = "libcurl";
    private const string BrotliCommonFileName = "brotlicommon";
    private const string BrotliDecFileName = "brotlidec";
    private const string BrotliEncFileName = "brotlienc";
    private const string LibSshFileName = "libssh2";
    private const string LibCryptoFileName = "libcrypto";
    private const string LibSslFileName = "libssl";
#endif

    private readonly bool _mustUseAnsi;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaInfo"/> class.
    /// </summary>
#if NETFRAMEWORK
    public MediaInfo() :
      this(Environment.Is64BitProcess ?
#if NET481_OR_GREATER
          RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ? @".\ARM64" : @".\x64" :
#else
          @".\x64" :
#endif
          @".\x86")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaInfo"/> class.
    /// </summary>
    /// <param name="pathToDll">The path to the directory containing the MediaInfo library.</param>
    public MediaInfo(string pathToDll)
    {
      NativeMethods.LoadLibraryEx(Path.Combine(pathToDll, BrotliCommonFileName), IntPtr.Zero, NativeMethods.LoadLibraryFlags.None);
      NativeMethods.LoadLibraryEx(Path.Combine(pathToDll, BrotliDecFileName), IntPtr.Zero, NativeMethods.LoadLibraryFlags.None);
      NativeMethods.LoadLibraryEx(Path.Combine(pathToDll, BrotliEncFileName), IntPtr.Zero, NativeMethods.LoadLibraryFlags.None);
      NativeMethods.LoadLibraryEx(Path.Combine(pathToDll, Environment.Is64BitProcess ?
#if NET481_OR_GREATER
        (RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ? LibCryptoFileNameArm64 : LibCryptoFileName64Bit) : LibCryptoFileName), IntPtr.Zero, NativeMethods.LoadLibraryFlags.None);
#else
        LibCryptoFileName64Bit : LibCryptoFileName), IntPtr.Zero, NativeMethods.LoadLibraryFlags.None);
#endif
      NativeMethods.LoadLibraryEx(Path.Combine(pathToDll, Environment.Is64BitProcess ?
#if NET481_OR_GREATER
        (RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ? LibSslFileNameArm64 : LibSslFileName64Bit) : LibSslFileName), IntPtr.Zero, NativeMethods.LoadLibraryFlags.None);
#else
        LibSslFileName64Bit : LibSslFileName), IntPtr.Zero, NativeMethods.LoadLibraryFlags.None);
#endif
      NativeMethods.LoadLibraryEx(Path.Combine(pathToDll, LibSshFileName), IntPtr.Zero, NativeMethods.LoadLibraryFlags.None);
      NativeMethods.LoadLibraryEx(Path.Combine(pathToDll, LibCurlFileName), IntPtr.Zero, NativeMethods.LoadLibraryFlags.None);
      _module = NativeMethods.LoadLibraryEx(Path.Combine(pathToDll, MediaInfoFileName), IntPtr.Zero, NativeMethods.LoadLibraryFlags.None);
      try
      {
        Handle = NativeMethods.MediaInfo_New();
      }
      catch
      {
        Handle = IntPtr.Zero;
      }

      _mustUseAnsi = Environment.OSVersion.ToString().IndexOf("Windows", StringComparison.OrdinalIgnoreCase) == -1;
    }
#else
    public MediaInfo()
    {
#if NET6_0_OR_GREATER
      // .NET 6.0 and later support native library dependencies, so we don't need to load libcurl manually. However, we still need to ensure that libcurl is
      // loaded before MediaInfo library, otherwise MediaInfo library will fail to load due to missing libcurl symbols. This is a workaround for the issue
      // in .NET 6.0 and later, which do not support native library dependencies in the same way as .NET Core 3.1 and .NET 5.0.
      var currentAssembly = typeof(MediaInfo).Assembly;
      NativeLibrary.TryLoad(LibCurlFileName, currentAssembly, null, out var libCurl);
      NativeLibrary.TryLoad(MediaInfoFileName, currentAssembly, null, out var libMediaInfo);
#else
#if NETSTANDARD2_1_OR_GREATER
      // Ensure that libcurl is loaded before MediaInfo library, otherwise MediaInfo library will fail to load due to missing libcurl symbols.
      // This is a workaround for the issue in .NET Core 3.1 and .NET 5.0, which do not support native library dependencies in the same way as .NET 6.0 and later.
      CurlNativeMethods.curl_version();
#endif
#endif
      try
      {
        Handle = NativeMethods.MediaInfo_New();
      }
      catch (Exception e)
      {
        Handle = IntPtr.Zero;
      }

      _mustUseAnsi = false;
    }
#endif
            /// <summary>
            /// Finalizes an instance of the <see cref="MediaInfo"/> class.
            /// </summary>
            ~MediaInfo()
    {
      Dispose(false);
    }

    /// <summary>
    /// Opens the specified file name to access to media stream data.
    /// </summary>
    /// <param name="fileName">The path to the media file.</param>
    /// <returns>Returns internal handle to access to low-level functions.</returns>
    public IntPtr Open(string fileName)
    {
      if (Handle == IntPtr.Zero)
      {
        return IntPtr.Zero;
      }

      return _mustUseAnsi ?
        NativeMethods.MediaInfoA_Open(Handle, fileName) :
        NativeMethods.MediaInfo_Open(Handle, fileName);
    }

    /// <summary>
    /// Gets the library handle to access to low-level functions.
    /// </summary>
    /// <remarks>
    /// This property is used to access to low-level functions, such as Open_Buffer_Init, Open_Buffer_Continue, Open_Buffer_Finalize, etc. If library is not loaded successfully, this property will return IntPtr.Zero.
    /// </remarks>
    public IntPtr Handle { get; private set; }

    /// <summary>
    /// Opens the buffer initialization to inform library about the file size and offset.
    /// </summary>
    /// <param name="fileSize">Size of the file.</param>
    /// <param name="fileOffset">The file offset.</param>
    /// <returns>Returns internal success code. If library is not loaded successfully, will return IntPtr.Zero.</returns>
    public IntPtr OpenBufferInit(long fileSize, long fileOffset) =>
      Handle == IntPtr.Zero ? IntPtr.Zero : NativeMethods.MediaInfo_Open_Buffer_Init(Handle, fileSize, fileOffset);

    /// <summary>
    /// Opens the buffer continue to inform library about the buffer content.
    /// </summary>
    /// <param name="buffer">The pointer to the buffer.</param>
    /// <param name="bufferSize">Size of the buffer.</param>
    /// <returns>Returns internal success code. If library is not loaded successfully, will return IntPtr.Zero.</returns>
    public IntPtr OpenBufferContinue(IntPtr buffer, IntPtr bufferSize) =>
      Handle == IntPtr.Zero ? IntPtr.Zero : NativeMethods.MediaInfo_Open_Buffer_Continue(Handle, buffer, bufferSize);

    /// <summary>
    /// Opens the buffer continue to inform library about the buffer content by unsafe code.
    /// </summary>
    /// <param name="buffer">The pointer to the buffer.</param>
    /// <param name="bufferSize">Size of the buffer.</param>
    /// <returns>Returns internal success code. If library is not loaded successfully, will return 0.</returns>
    public unsafe int OpenBufferContinue(byte* buffer, int bufferSize) =>
      Handle == IntPtr.Zero ? 0 : (int) NativeMethods.MediaInfo_Open_Buffer_Continue(Handle, buffer, (IntPtr) bufferSize);

    /// <summary>
    /// Opens the buffer to continue to get the position to go to.
    /// </summary>
    /// <returns>Returns the position to go to. If library is not loaded successfully, will return 0.</returns>
    public long OpenBufferContinueGoToGet() =>
      Handle == IntPtr.Zero ? 0 : NativeMethods.MediaInfo_Open_Buffer_Continue_GoTo_Get(Handle);

    /// <summary>
    /// Opens the buffer finalize.
    /// </summary>
    /// <returns>Returns internal success code. If library is not loaded successfully, will return IntPtr.Zero.</returns>
    public IntPtr OpenBufferFinalize() =>
      Handle == IntPtr.Zero ? IntPtr.Zero : NativeMethods.MediaInfo_Open_Buffer_Finalize(Handle);

    /// <summary>
    /// Closes this instance and releases library resources.
    /// </summary>
    public void Close()
    {
      if (Handle != IntPtr.Zero)
      {
        NativeMethods.MediaInfo_Delete(Handle);
        Handle = IntPtr.Zero;
      }
    }

    /// <summary>
    /// Informs media stream data in the file. If library is not loaded successfully, will return empty string.
    /// </summary>
    /// <returns>
    /// Returns media informs in case library loaded successfully; elsewhere will return empty string.
    /// </returns>
    public string Inform()
    {
      if (Handle == IntPtr.Zero)
      {
        return string.Empty;
      }

      return _mustUseAnsi ?
        NativeMethods.MediaInfoA_Inform(Handle, IntPtr.Zero) :
        NativeMethods.MediaInfo_Inform(Handle, IntPtr.Zero);
    }

    /// <summary>
    /// Gets the specified parameter value in the stream by parameter name and kind of information to return. If library is not loaded successfully, will return empty string.
    /// </summary>
    /// <param name="streamKind">Kind of the stream.</param>
    /// <param name="streamNumber">The stream number.</param>
    /// <param name="parameter">The parameter.</param>
    /// <param name="kindOfInfo">The kind of information.</param>
    /// <param name="kindOfSearch">The kind of search.</param>
    /// <returns>
    /// Returns the specified parameter value in the stream in case library loaded successfully; elsewhere will return empty string.
    /// </returns>
    public string Get(StreamKind streamKind, int streamNumber, string parameter, InfoKind kindOfInfo, InfoKind kindOfSearch)
    {
      if (Handle == IntPtr.Zero)
      {
        return string.Empty;
      }

      return _mustUseAnsi ?
        NativeMethods.MediaInfoA_Get(
          Handle,
          (IntPtr)streamKind,
          (IntPtr)streamNumber,
          parameter,
          (IntPtr)kindOfInfo,
          (IntPtr)kindOfSearch) :
        NativeMethods.MediaInfo_Get(
          Handle,
          (IntPtr)streamKind,
          (IntPtr)streamNumber,
          parameter,
          (IntPtr)kindOfInfo,
          (IntPtr)kindOfSearch);
    }

    /// <summary>
    /// Gets the specified parameter value in the stream by parameter index and kind of information to return. If library is not loaded successfully, will return empty string.
    /// </summary>
    /// <param name="streamKind">Kind of the stream.</param>
    /// <param name="streamNumber">The stream number.</param>
    /// <param name="parameter">The parameter index.</param>
    /// <param name="kindOfInfo">The kind of information.</param>
     /// <returns>
     /// Returns the specified parameter value in the stream in case library loaded successfully; elsewhere will return empty string.
     /// </returns>
    public string Get(StreamKind streamKind, int streamNumber, int parameter, InfoKind kindOfInfo)
    {
      if (Handle == IntPtr.Zero)
      {
        return string.Empty;
      }

      return _mustUseAnsi ?
        NativeMethods.MediaInfoA_GetI(Handle, (IntPtr)streamKind, (IntPtr)streamNumber, (IntPtr)parameter, (IntPtr)kindOfInfo) :
        NativeMethods.MediaInfo_GetI(Handle, (IntPtr)streamKind, (IntPtr)streamNumber, (IntPtr)parameter, (IntPtr)kindOfInfo);
    }

    /// <summary>
    /// Gets options value by the specified option name and value. If library is not loaded successfully, will return empty string.
    /// </summary>
    /// <param name="option">The option name.</param>
    /// <param name="value">The option value.</param>
    /// <returns>
    /// Returns the specified option value in case library loaded successfully; elsewhere will return empty string.
    /// </returns>
    public string Option(string option, string value)
    {
      if (Handle == IntPtr.Zero)
      {
        return string.Empty;
      }

      return _mustUseAnsi ?
        NativeMethods.MediaInfoA_Option(Handle, option, value) :
        NativeMethods.MediaInfo_Option(Handle, option, value);
    }

    /// <summary>
    /// Gets current state. If library is not loaded successfully, will return IntPtr.Zero.
    /// </summary>
    /// <returns>
    /// Returns the current state in case library loaded successfully; elsewhere will return IntPtr.Zero.
    /// </returns>
    public IntPtr StateGet() =>
      Handle == IntPtr.Zero ? IntPtr.Zero : NativeMethods.MediaInfo_State_Get(Handle);

    /// <summary>
    /// Gets count of items in stream by stream kind and stream number. If library is not loaded successfully, will return 0.
    /// </summary>
    /// <param name="streamKind">Kind of the stream.</param>
    /// <param name="streamNumber">The stream number.</param>
    /// <returns>
    /// Returns the count of items in case library loaded successfully; elsewhere will return 0.
    /// </returns>
    public int CountGet(StreamKind streamKind, int streamNumber) =>
      Handle == IntPtr.Zero ? 0 : (int)NativeMethods.MediaInfo_Count_Get(Handle, (IntPtr)streamKind, (IntPtr)streamNumber);

    /// <summary>
    /// Gets the specified parameter value in the stream by parameter name and kind of information to return. If library is not loaded successfully, will return empty string.
    /// </summary>
    /// <param name="streamKind">Kind of the stream.</param>
    /// <param name="streamNumber">The stream number.</param>
    /// <param name="parameter">The parameter.</param>
    /// <param name="kindOfInfo">The kind of information.</param>
    /// <returns>
    /// Returns the specified parameter value in the stream in case library loaded successfully; elsewhere will return empty string.
    /// </returns>
    public string Get(StreamKind streamKind, int streamNumber, string parameter, InfoKind kindOfInfo) =>
      Get(streamKind, streamNumber, parameter, kindOfInfo, InfoKind.Name);

    /// <summary>
    /// Gets the specified parameter value in the stream by parameter name. If library is not loaded successfully, will return empty string.
    /// </summary>
    /// <param name="streamKind">Kind of the stream.</param>
    /// <param name="streamNumber">The stream number.</param>
    /// <param name="parameter">The parameter.</param>
    /// <returns>
    /// Returns the specified parameter value in the stream in case library loaded successfully; elsewhere will return empty string.
    /// </returns>
    public string Get(StreamKind streamKind, int streamNumber, string parameter) =>
      Get(streamKind, streamNumber, parameter, InfoKind.Text, InfoKind.Name);

    /// <summary>
    /// Gets the specified parameter value in the stream by parameter index. If library is not loaded successfully, will return empty string.
    /// </summary>
    /// <param name="streamKind">Kind of the stream.</param>
    /// <param name="streamNumber">The stream number.</param>
    /// <param name="parameter">The parameter.</param>
    /// <returns>
    /// Returns the specified parameter value in the stream in case library loaded successfully; elsewhere will return empty string.
    /// </returns>
    public string Get(StreamKind streamKind, int streamNumber, int parameter) =>
      Get(streamKind, streamNumber, parameter, InfoKind.Text);

    /// <summary>
    /// Gets options value by the specified option name.
    /// </summary>
    /// <param name="option">The option.</param>
    /// <returns>
    /// Returns the options value in case library loaded successfully; elsewhere will return empty string.
    /// </returns>
    public string Option(string option) =>
      Option(option, string.Empty);

    /// <summary>
    /// Gets count of specified kind of stream in the file. If library is not loaded successfully, will return 0.
    /// </summary>
    /// <param name="streamKind">Kind of the streams.</param>
    /// <returns>
    /// Returns the count of specified kind of streams in case library loaded successfully; elsewhere will return 0.
    /// </returns>
    public int CountGet(StreamKind streamKind) =>
      CountGet(streamKind, -1);

    /// <inheritdoc/>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases unmanaged resources.
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
      Close();
#if NETFRAMEWORK
      if (_module != IntPtr.Zero)
      {
        NativeMethods.FreeLibrary(_module);
        _module = IntPtr.Zero;
      }
#endif
    }
  }

  /// <summary>
  /// Describes low-level function to access to mediaInfo lists. If library is not loaded successfully, will return empty string.
  /// </summary>
  /// <seealso cref="IDisposable" />
  public class MediaInfoList : IDisposable
  {
    private readonly bool _useAnsiStrings;
    private IntPtr _handle;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaInfoList"/> class.
    /// </summary>
    /// <param name="useAnsiStrings">if set to <c>true</c> will use ANSI strings; otherwise, Unicode strings will be used.</param>
    public MediaInfoList(bool useAnsiStrings)
    {
      _useAnsiStrings = useAnsiStrings;
      _handle = NativeMethods.MediaInfoList_New();
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="MediaInfoList"/> class.
    /// </summary>
    ~MediaInfoList()
    {
      Dispose(false);
    }

    /// <summary>
    /// Opens the specified file name to access to media stream data in the list. If library is not loaded successfully, will return -1.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="options">The options.</param>
    /// <returns>
    /// Returns the file position in case library loaded successfully; elsewhere will return -1.
    /// </returns>
    public int Open(string fileName, InfoFileOptions options) =>
      _useAnsiStrings ?
        (int)NativeMethods.MediaInfoListA_Open(_handle, fileName, (IntPtr)options) :
        (int)NativeMethods.MediaInfoList_Open(_handle, fileName, (IntPtr)options);

    /// <summary>
    /// Closes the specified file position in the list. If library is not loaded successfully, will do nothing.
    /// </summary>
    /// <param name="filePos">The file position to close. If filePos is -1, all file positions will be closed.</param>
    public void Close(int filePos) =>
      NativeMethods.MediaInfoList_Close(_handle, (IntPtr)filePos);

    /// <summary>
    /// Informs the specified file position.
    /// </summary>
    /// <param name="filePos">The file position.</param>
    /// <returns>
    /// Returns the information in case library loaded successfully; elsewhere will return empty string.
    /// </returns>
    public string Inform(int filePos) =>
      _useAnsiStrings ?
        NativeMethods.MediaInfoListA_Inform(_handle, (IntPtr)filePos, IntPtr.Zero) :
        NativeMethods.MediaInfoList_Inform(_handle, (IntPtr)filePos, IntPtr.Zero);

    /// <summary>
    /// Gets the property value in specified file position by stream and property name.
    /// </summary>
    /// <param name="filePos">The file position.</param>
    /// <param name="streamKind">Kind of the stream.</param>
    /// <param name="streamNumber">The stream number.</param>
    /// <param name="parameter">The property name.</param>
    /// <param name="kindOfInfo">The kind of information.</param>
    /// <param name="kindOfSearch">The kind of search.</param>
    /// <returns>
    /// Returns the specified property value in the stream in case library loaded successfully; elsewhere will return empty string.
    /// </returns>
    public string Get(int filePos, StreamKind streamKind, int streamNumber, string parameter, InfoKind kindOfInfo, InfoKind kindOfSearch) =>
      _useAnsiStrings ?
        NativeMethods.MediaInfoListA_Get(_handle, (IntPtr)filePos, (IntPtr)streamKind, (IntPtr)streamNumber, parameter, (IntPtr)kindOfInfo, (IntPtr)kindOfSearch) :
        NativeMethods.MediaInfoList_Get(_handle, (IntPtr)filePos, (IntPtr)streamKind, (IntPtr)streamNumber, parameter, (IntPtr)kindOfInfo, (IntPtr)kindOfSearch);

    /// <summary>
    /// Gets the property value in specified file position by stream and property index.
    /// </summary>
    /// <param name="filePos">The file position.</param>
    /// <param name="streamKind">Kind of the stream.</param>
    /// <param name="streamNumber">The stream number.</param>
    /// <param name="parameter">The property index.</param>
    /// <param name="kindOfInfo">The kind of information.</param>
    /// <returns>
    /// Returns the specified property value in the stream in case library loaded successfully; elsewhere will return empty string.
    /// </returns>
    public string Get(int filePos, StreamKind streamKind, int streamNumber, int parameter, InfoKind kindOfInfo) =>
      _useAnsiStrings ?
        NativeMethods.MediaInfoListA_GetI(_handle, (IntPtr)filePos, (IntPtr)streamKind, (IntPtr)streamNumber, (IntPtr)parameter, (IntPtr)kindOfInfo) :
        NativeMethods.MediaInfoList_GetI(_handle, (IntPtr)filePos, (IntPtr)streamKind, (IntPtr)streamNumber, (IntPtr)parameter, (IntPtr)kindOfInfo);

    /// <summary>
    /// Sets options value by the specified option name.
    /// </summary>
    /// <param name="option">The option name.</param>
    /// <param name="value">The option value.</param>
    /// <returns>
    /// Returns the specified option value in case library loaded successfully; elsewhere will return empty string.
    /// </returns>
    public string Option(string option, string value) =>
      _useAnsiStrings ?
        NativeMethods.MediaInfoListA_Option(_handle, option, value) :
        NativeMethods.MediaInfoList_Option(_handle, option, value);

    /// <summary>
    /// Gets current state.
    /// </summary>
    /// <returns>
    /// Returns the current state in case library loaded successfully; elsewhere will return IntPtr.Zero.
    /// </returns>
    public int StateGet() =>
      (int)NativeMethods.MediaInfoList_State_Get(_handle);

    /// <summary>
    /// Gets count of items in file position and stream.
    /// </summary>
    /// <param name="filePos">The file position.</param>
    /// <param name="streamKind">Kind of the stream.</param>
    /// <param name="streamNumber">The stream number.</param>
    /// <returns>
    /// Returns the count of items in the specified file position and stream in case library loaded successfully; elsewhere will return 0.
    /// </returns>
    public int CountGet(int filePos, StreamKind streamKind, int streamNumber) =>
      (int)NativeMethods.MediaInfoList_Count_Get(_handle, (IntPtr)filePos, (IntPtr)streamKind, (IntPtr)streamNumber);

    /// <summary>
    /// Opens the specified file name to access to media stream data in the list with default options (InfoFileOptions.Nothing).
    /// </summary>
    /// <param name="fileName">The name of the file.</param>
    public void Open(string fileName) =>
      Open(fileName, 0);

    /// <summary>
    /// Closes the specified file position in the list with default value -1, which means all file positions will be closed.
    /// </summary>
    public void Close() =>
      Close(-1);

    /// <summary>
    /// Informs the specified file position with default value 0.
    /// </summary>
    /// <param name="filePos">The file position.</param>
    /// <param name="streamKind">Kind of the stream.</param>
    /// <param name="streamNumber">The stream number.</param>
    /// <param name="parameter">The parameter.</param>
    /// <param name="kindOfInfo">The kind of information.</param>
    /// <returns>
    /// Returns the specified property value in the stream in case library loaded successfully; elsewhere will return empty string.
    /// </returns>
    public string Get(int filePos, StreamKind streamKind, int streamNumber, string parameter, InfoKind kindOfInfo) =>
      Get(filePos, streamKind, streamNumber, parameter, kindOfInfo, InfoKind.Name);

    /// <summary>
    /// Gets the specified file position.
    /// </summary>
    /// <param name="filePos">The file position.</param>
    /// <param name="streamKind">Kind of the stream.</param>
    /// <param name="streamNumber">The stream number.</param>
    /// <param name="parameter">The parameter.</param>
    /// <returns>
    /// Returns the specified property value in the stream in case library loaded successfully; elsewhere will return empty string.
    /// </returns>
    public string Get(int filePos, StreamKind streamKind, int streamNumber, string parameter) =>
      Get(filePos, streamKind, streamNumber, parameter, InfoKind.Text, InfoKind.Name);

    /// <summary>
    /// Gets the specified file position.
    /// </summary>
    /// <param name="filePos">The file position.</param>
    /// <param name="streamKind">Kind of the stream.</param>
    /// <param name="streamNumber">The stream number.</param>
    /// <param name="parameter">The parameter.</param>
    /// <returns>
    /// Returns the specified property value in the stream in case library loaded successfully; elsewhere will return empty string.
    /// </returns>
    public string Get(int filePos, StreamKind streamKind, int streamNumber, int parameter) =>
      Get(filePos, streamKind, streamNumber, parameter, InfoKind.Text);

    /// <summary>
    /// Gets options value by the specified option name.
    /// </summary>
    /// <param name="option">The option name.</param>
    /// <returns>
    /// Returns the specified option value in case library loaded successfully; elsewhere will return empty string.
    /// </returns>
    public string Option(string option) =>
      Option(option, string.Empty);

    /// <summary>
    /// Gets count of specified kind of stream in th file position.
    /// </summary>
    /// <param name="filePos">The file position.</param>
    /// <param name="streamKind">Kind of the streams.</param>
    /// <returns>
    /// Returns the count of items in the specified file position and stream in case library loaded successfully; elsewhere will return 0.
    /// </returns>
    public int CountGet(int filePos, StreamKind streamKind) =>
      CountGet(filePos, streamKind, -1);

    /// <inheritdoc/>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases unmanaged resources.
    /// </summary>
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
      if (_handle != IntPtr.Zero)
      {
        NativeMethods.MediaInfoList_Delete(_handle);
        _handle = IntPtr.Zero;
      }
    }
  }
}