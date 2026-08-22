#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

using MediaInfo.Model;

namespace MediaInfo.Builder
{
  /// <summary>
  /// Provides a base class for building language-specific media streams with additional language metadata.
  /// </summary>
  /// <remarks>
  /// This class extends <see cref="MediaStreamBuilder{TStream}"/> to support streams that include language information, such
  /// as audio or subtitle tracks. It populates language-related properties on the resulting stream instance during the
  /// build process.
  /// </remarks>
  /// <typeparam name="TStream">The type of media stream to build. Must inherit from <see cref="LanguageMediaStream"/> and have a parameterless constructor.</typeparam>
  internal abstract class LanguageMediaStreamBuilder<TStream> : MediaStreamBuilder<TStream> where TStream : LanguageMediaStream, new()
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="LanguageMediaStreamBuilder{TStream}"/> class.
    /// </summary>
    /// <param name="info">The media info object.</param>
    /// <param name="number">The stream number.</param>
    /// <param name="position">The stream position.</param>
    protected LanguageMediaStreamBuilder(IMediaInfoReader info, int number, int position)
      : base(info, number, position)
    {
    }

    /// <inheritdoc />
    public override TStream Build()
    {
      var result = base.Build();
      var language = Get("Language").ToLower();
      result.Language = LanguageHelper.GetLanguageByShortName(language);
      result.LanguageIetf = Get("LanguageIETF");
      result.Default = Get<bool>("Default", TagBuilderHelper.TryGetBool);
      result.Forced = Get<bool>("Forced", TagBuilderHelper.TryGetBool);
      result.Lcid = LanguageHelper.GetLcidByShortName(language);
      result.StreamSize = Get<long>("StreamSize", TagBuilderHelper.TryGetLong);
      return result;
    }
  }
}