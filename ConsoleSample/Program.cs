using MediaInfo;
using Microsoft.Extensions.Logging;

var logger = LoggerFactory.Create(a => { }).CreateLogger<Program>();
var wrapper = new MediaInfoWrapper("videos/video_with_rotation.mp4", logger);

Console.WriteLine(wrapper.VideoRotation);