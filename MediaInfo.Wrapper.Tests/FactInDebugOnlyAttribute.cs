#region Copyright (C) 2017-2022 Yaroslav Tatarenko

// Copyright (C) 2017-2022 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2020 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

using System.Diagnostics;
using Xunit;

namespace MediaInfo.Wrapper.Tests
{
  public class FactInDebugOnlyAttribute : FactAttribute
  {
    public FactInDebugOnlyAttribute()
    {
      if (!Debugger.IsAttached)
      {
        Skip = "Only running in interactive mode.";
      }
    }
  }
}
