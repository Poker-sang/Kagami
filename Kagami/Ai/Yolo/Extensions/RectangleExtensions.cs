using SixLabors.ImageSharp;

namespace Kagami.Ai.Yolo.Extensions
{
    public static class RectangleExtensions
    {
        public static float Area(this RectangleF source) => source.Width * source.Height;
    }
}
