#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using MediaInfo.Model;

namespace MediaInfo.Builder
{
  /// <summary>
  /// Builds the IPTC metadata of a file from the general fields the library fills from IPTC-IIM and XMP.
  /// </summary>
  /// <param name="info">The media information source.</param>
  internal class IptcInfoBuilder(IMediaInfoReader info)
  {
    /// <summary>
    /// Builds the IPTC metadata.
    /// </summary>
    /// <returns>Returns the metadata, or <see langword="null"/> when the file has none.</returns>
    public IptcInfo? Build()
    {
      var result = new IptcInfo
      {
        Headline = Get(IptcField.Headline),
        Credit = Get(IptcField.Credit),
        Source = Get(IptcField.Source),
        CreatorJobTitle = Get(IptcField.CreatorJobTitle),
        DescriptionWriter = Get(IptcField.DescriptionWriter),
        Contact = Get(IptcField.Contact),
        Instructions = Get(IptcField.Instructions),
        JobId = Get(IptcField.JobId),
        Urgency = Get(IptcField.Urgency),
        Category = Get(IptcField.Category),
        SupplementalCategories = Get(IptcField.SupplementalCategories),
        IntellectualGenre = Get(IptcField.IntellectualGenre),
        SubjectCode = Get(IptcField.SubjectCode),
        SceneCode = Get(IptcField.SceneCode),
        SubjectReference = Get(IptcField.SubjectReference),
        ObjectAttributeReference = Get(IptcField.ObjectAttributeReference),
        EditStatus = Get(IptcField.EditStatus),
        FixtureIdentifier = Get(IptcField.FixtureIdentifier),
        ObjectCycle = Get(IptcField.ObjectCycle),
        City = Get(IptcField.City),
        Sublocation = Get(IptcField.Sublocation),
        State = Get(IptcField.State),
        CountryCode = Get(IptcField.CountryCode),
        ContentLocationCode = Get(IptcField.ContentLocationCode),
        ContentLocationName = Get(IptcField.ContentLocationName),
        ExpirationDate = Get(IptcField.ExpirationDate),
        DigitalSourceType = Get(IptcField.DigitalSourceType),
        DigitalSourceTypeName = Get(IptcField.DigitalSourceType_String)
      };

      return IsEmpty(result) ? null : result;
    }

    private string Get(IptcField field) =>
      info.Get(StreamKind.General, 0, field.ToParameterName()) ?? string.Empty;

    private static bool IsEmpty(IptcInfo value) =>
      value.Headline.Length + value.Credit.Length + value.Source.Length + value.CreatorJobTitle.Length +
      value.DescriptionWriter.Length + value.Contact.Length + value.Instructions.Length + value.JobId.Length +
      value.Urgency.Length + value.Category.Length + value.SupplementalCategories.Length +
      value.IntellectualGenre.Length + value.SubjectCode.Length + value.SceneCode.Length +
      value.SubjectReference.Length + value.ObjectAttributeReference.Length + value.EditStatus.Length +
      value.FixtureIdentifier.Length + value.ObjectCycle.Length + value.City.Length + value.Sublocation.Length +
      value.State.Length + value.CountryCode.Length + value.ContentLocationCode.Length +
      value.ContentLocationName.Length + value.ExpirationDate.Length + value.DigitalSourceType.Length +
      value.DigitalSourceTypeName.Length == 0;
  }
}
