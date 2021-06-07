#region Copyright (C) 2017-2021 Yaroslav Tatarenko

// Copyright (C) 2017-2021 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2021 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

using AutoMapper;
using MediaInfo;
using MediaInfo.Model;
using System;

namespace ApiSample.Infrastructure
{
    internal static class MapperExtensions
    {
        public static void ConfigureMapper(this IMapperConfigurationExpression config)
        {
            config.CreateMap<MediaInfoWrapper, Models.MediaInfo>()
                .ForMember(x => x.Duration, x => x.MapFrom(y => TimeSpan.FromSeconds(y.Duration)));
            config.CreateMap<BaseTags, Models.BaseTags>();
            config.CreateMap<AspectRatio, Models.AspectRatio>();
            config.CreateMap<AudioCodec, Models.AudioCodec>();
            config.CreateMap<LanguageMediaStream, Models.LanguageMediaStream>();
            config.CreateMap<AudioStream, Models.AudioStream>();
            config.CreateMap<AudioTags, Models.AudioTags>();
            config.CreateMap<BitrateMode, Models.BitrateMode>();
            config.CreateMap<ChapterStream, Models.ChapterStream>();
            config.CreateMap<CoverInfo, Models.CoverInfo>();
            config.CreateMap<Chapter, Models.Chapter>();
            config.CreateMap<ChromaSubSampling, Models.ChromaSubSampling>();
            config.CreateMap<ColorSpace, Models.ColorSpace>();
            config.CreateMap<Hdr, Models.Hdr>();
            config.CreateMap<MediaStreamKind, Models.MediaStreamKind>();
            config.CreateMap<MenuStream, Models.MenuStream>();
            config.CreateMap<StereoMode, Models.StereoMode>();
            config.CreateMap<SubtitleCodec, Models.SubtitleCodec>();
            config.CreateMap<SubtitleStream, Models.SubtitleStream>();
            config.CreateMap<TransferCharacteristic, Models.TransferCharacteristic>();
            config.CreateMap<VideoCodec, Models.VideoCodec>();
            config.CreateMap<VideoStandard, Models.VideoStandard>();
            config.CreateMap<VideoStream, Models.VideoStream>();
            config.CreateMap<VideoTags, Models.VideoTags>();
        }
    }
}
