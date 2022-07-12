using Microsoft.ML.OnnxRuntime.Tensors;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Kagami.Ai;

internal static class Utilities
{
    internal static float Area(this RectangleF source) => source.Width * source.Height;

    /// <summary>
    /// Extracts pixels into tensor for net input.
    /// </summary>
    internal static Tensor<float> ExtractPixels(this Image<Rgba32> image)
    {
        var tensor = new DenseTensor<float>(new[] { 1, 3, image.Height, image.Width });

        _ = Parallel.For(0, image.Height, y =>
            Parallel.For(0, image.Width, x =>
            {
                tensor[0, 0, y, x] = image[x, y].R / 255.0F; // r
                tensor[0, 1, y, x] = image[x, y].G / 255.0F; // g
                tensor[0, 2, y, x] = image[x, y].B / 255.0F; // b
            }));

        return tensor;
    }
}
