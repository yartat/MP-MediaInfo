using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ApiSample.Models
{
    /// <summary>
    /// Request parameters to retrieve media info
    /// </summary>
    [DataContract]
    public class MediaInfoRequest
    {
        /// <summary>
        /// A location of the media to retrieve info.
        /// </summary>
        /// <example>/app/Data/Test_H264.m2ts</example>
        [DataMember(Name = "location")]
        [JsonPropertyName("location")]
        [Required(ErrorMessage = "LOCATION_REQUIRED")]
        public Uri Location { get; set; }
    }
}
