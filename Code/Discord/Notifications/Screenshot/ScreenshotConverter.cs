using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace PoolAlerter.Code.Discord.Notifications.Screenshot
{
    internal class ScreenshotConverter : IScreenshotConverter
    {
        public Stream ConvertToJpeg(byte[] screenshot)
        {
            var memoryStream = new MemoryStream(screenshot);
            
            var image = Image.FromStream(memoryStream);
            var result = new MemoryStream();
            image.Save(result, ImageFormat.Jpeg);

            result.Seek(0, SeekOrigin.Begin);
            return result;
        }
    }
}