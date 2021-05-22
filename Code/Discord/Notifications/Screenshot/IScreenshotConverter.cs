using System.IO;

namespace PoolAlerter.Code.Discord.Notifications.Screenshot
{
    internal interface IScreenshotConverter
    {
        public Stream ConvertToJpeg(byte[] screenshot);
    }
}