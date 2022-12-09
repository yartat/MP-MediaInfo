#region Copyright (C) 2017-2022 Yaroslav Tatarenko

// Copyright (C) 2017-2022 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2021 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

using System;
using System.Runtime.InteropServices;

namespace MediaInfo;

/// <summary>
/// Describes methods to work with unmanaged Global memory block
/// </summary>
/// <seealso cref="IDisposable" />
public class GlobalMemory : IDisposable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GlobalMemory"/> class.
    /// </summary>
    /// <param name="handle">The handle.</param>
    private GlobalMemory(IntPtr handle)
    {
        Handle = handle;
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="GlobalMemory"/> class.
    /// </summary>
    ~GlobalMemory()
    {
        Dispose(false);
    }

    /// <summary>
    /// The handle of the memory allocation.
    /// </summary>
    public IntPtr Handle { get; private set; }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Strings to global ANSI string.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <returns></returns>
    public static GlobalMemory StringToGlobalAnsi(string source) =>
        new(Marshal.StringToHGlobalAnsi(source));

    private void Dispose(bool _)
    {
        if (Handle != IntPtr.Zero)
        {
            Marshal.FreeHGlobal(Handle);
            Handle = IntPtr.Zero;
        }
    }
}
