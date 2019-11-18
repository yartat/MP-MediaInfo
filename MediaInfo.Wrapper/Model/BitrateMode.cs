namespace MediaInfo.Model
{
    /// <summary>
    /// Describes bitrate possible modes
    /// </summary>
    public enum BitrateMode : byte
    {
        /// <summary>
        /// Constant quality mode
        /// </summary>
        Cq,

        /// <summary>
        /// Constant bitrate mode 
        /// </summary>
        Cbr,

        /// <summary>
        /// Variable bitrate mode
        /// </summary>
        Vbr
    }
}
