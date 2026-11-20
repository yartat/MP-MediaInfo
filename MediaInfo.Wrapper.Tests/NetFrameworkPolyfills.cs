#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

#if NETFRAMEWORK
namespace System.Runtime.CompilerServices
{
  /// <summary>
  /// Lets the CsvHelper source generator register its mappers on .NET Framework, which has no such attribute.
  /// </summary>
  [AttributeUsage(AttributeTargets.Method, Inherited = false)]
  internal sealed class ModuleInitializerAttribute : Attribute
  {
  }
}
#endif
