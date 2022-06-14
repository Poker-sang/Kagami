using Kagami.Ai.Yolo;
using Kagami.Ai.Yolo.Models;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Kagami.Services;

public static class ArtificialIntelligence
{

    public static async Task<Stream> Yolo(Stream stream)
    {
        using var image = await Image.LoadAsync<Rgba32>(stream);

        using var scorer = new YoloScorer<YoloCocoP5Model>("Assets/yolov5n.onnx");

        var img = image.Clone();

        var predictions = scorer.Predict(img);

        var font = new Font(new FontCollection().Add("Assets/consola.ttf"), 16);
        foreach (var prediction in predictions) // iterate predictions to draw results
        {
            var score = Math.Round(prediction.Score, 2);

            var (x, y) = (prediction.Rectangle.Left - 3, prediction.Rectangle.Top - 23);

            img.Mutate(a => a.DrawPolygon(new Pen(prediction.Label.Color, 1),
                new PointF(prediction.Rectangle.Left, prediction.Rectangle.Top),
                new PointF(prediction.Rectangle.Right, prediction.Rectangle.Top),
                new PointF(prediction.Rectangle.Right, prediction.Rectangle.Bottom),
                new PointF(prediction.Rectangle.Left, prediction.Rectangle.Bottom)
            ).DrawText($"{prediction.Label.Name} ({score})",
                font,
                prediction.Label.Color,
                new PointF(x, y)));
        }

        var ms = new MemoryStream();
        await img.SaveAsync(ms, new PngEncoder());
        return ms;
    }
}
