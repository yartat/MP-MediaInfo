#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

using Mapster;
using MediaInfo.Analysis.Results;
using System;

namespace ApiSample.Infrastructure;

internal class MapperRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // LegacyResultAdapter presents the immutable analysis result under the same property names the wrapper
        // used, so migrating the API meant changing where the object comes from, not this mapping.
        config.NewConfig<LegacyResultAdapter, Models.MediaInfo>()
            .Map(dest => dest.Duration, src => TimeSpan.FromMilliseconds(src.Duration));
    }
}
