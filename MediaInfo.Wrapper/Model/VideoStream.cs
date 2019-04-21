#region Copyright (C) 2005-2019 Team MediaPortal

// Copyright (C) 2005-2019 Team MediaPortal
// http://www.team-mediaportal.com
//
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
//
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion Copyright (C) 2005-2019 Team MediaPortal

using System;
using System.Drawing;

using JetBrains.Annotations;

namespace MediaInfo
{
    /// <summary>
    /// Describes properties of the video stream and method to analyze stream
    /// </summary>
    /// <seealso cref="LanguageMediaStream" />
    [PublicAPI]
    public class VideoStream : LanguageMediaStream
    {
        /// <summary>
        /// Gets or sets the video aspect ratio.
        /// </summary>
        /// <value>
        /// The video aspect ratio.
        /// </value>
        public AspectRatio AspectRatio { get; set; }

        /// <summary>
        /// Gets or sets the video bit depth.
        /// </summary>
        /// <value>
        /// The video bit depth.
        /// </value>
        public int BitDepth { get; set; }

        /// <summary>
        /// Gets or sets the video bitrate.
        /// </summary>
        /// <value>
        /// The video bitrate.
        /// </value>
        public double Bitrate { get; set; }

        /// <summary>
        /// Gets or sets the video codec.
        /// </summary>
        /// <value>
        /// The video codec.
        /// </value>
        public VideoCodec Codec { get; set; }

        /// <summary>
        /// Gets or sets the name of the video codec.
        /// </summary>
        /// <value>
        /// The name of the video codec.
        /// </value>
        public string CodecName { get; set; }

        /// <summary>
        /// Gets or sets the stream duration.
        /// </summary>
        /// <value>
        /// The stream duration.
        /// </value>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// Gets or sets the video format.
        /// </summary>
        /// <value>
        /// The video format.
        /// </value>
        public string Format { get; set; }

        /// <summary>
        /// Gets or sets the video frame rate.
        /// </summary>
        /// <value>
        /// The video frame rate.
        /// </value>
        public double FrameRate { get; set; }

        /// <summary>
        /// Gets or sets the frame rate mode.
        /// </summary>
        /// <value>
        /// The video frame rate mode.
        /// </value>
        public FrameRateMode FrameRateMode { get; set; }

        /// <summary>
        /// Gets or sets the video height.
        /// </summary>
        /// <value>
        /// The video height.
        /// </value>
        public int Height { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="VideoStream"/> is interlaced.
        /// </summary>
        /// <value>
        ///   <c>true</c> if interlaced; otherwise, <c>false</c>.
        /// </value>
        public bool Interlaced { get; set; }

        /// <inheritdoc />
        public override MediaStreamKind Kind => MediaStreamKind.Video;

        /// <summary>
        /// Gets the video resolution.
        /// </summary>
        /// <value>
        /// The video resolution.
        /// </value>
        public string Resolution => GetVideoResolution();

        /// <summary>
        /// Gets the video size.
        /// </summary>
        /// <value>
        /// The vidoe size.
        /// </value>
        public Size Size => new Size(Width, Height);

        /// <summary>
        /// Gets or sets the video stereoscopic mode.
        /// </summary>
        /// <value>
        /// The video stereoscopic mode.
        /// </value>
        public StereoMode Stereoscopic { get; set; }

        /// <summary>
        /// Gets the video stream tags.
        /// </summary>
        /// <value>
        /// The video stream tags.
        /// </value>
        public VideoTags Tags { get; internal set; } = new VideoTags();

        /// <summary>
        /// Gets or sets the video width.
        /// </summary>
        /// <value>
        /// The video width.
        /// </value>
        public int Width { get; set; }

        /// <inheritdoc />
        protected override StreamKind StreamKind => StreamKind.Video;

        private string GetVideoResolution()
        {
            string result;

            if (Width >= 7680 || Height >= 4320)
            {
                result = "4320";
            }
            else if (Width >= 3840 || Height >= 2160)
            {
                result = "2160";
            }
            else if (Width >= 1920 || Height >= 1080)
            {
                result = "1080";
            }
            else if (Width >= 1280 || Height >= 720)
            {
                result = "720";
            }
            else if (Height >= 576)
            {
                result = "576";
            }
            else if (Height >= 480)
            {
                result = "480";
            }
            else if (Height >= 360)
            {
                result = "360";
            }
            else if (Height >= 240)
            {
                result = "240";
            }
            else
            {
                result = "SD";
            }

            if (result != "SD")
            {
                result += Interlaced ? "I" : "P";
            }

            return result;
        }
    }
}