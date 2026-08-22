#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

#if NET7_0_OR_GREATER
#else

namespace System.Diagnostics.CodeAnalysis;

/// <summary>
/// Specifies that a method sets all required members for a class type.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Constructor)]
public class SetsRequiredMembersAttribute : Attribute
{
}
#endif