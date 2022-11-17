using Kagami.Ai.MobileNet;
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
    private static readonly Font _font = new(new FontCollection().Add("Assets/consola.ttf"), 16);

    public static async Task<Stream> Yolo(Stream stream)
    {
        using var image = await Image.LoadAsync<Rgba32>(stream);

        var scorer = new YoloScorer<YoloCocoP5Model>("Assets/yolov5n.onnx");

        var predictions = scorer.Predict(image);

        foreach (var prediction in predictions) // iterate predictions to draw results
        {
            var score = Math.Round(prediction.Score, 2);

            var (x, y) = (prediction.Rectangle.Left - 3, prediction.Rectangle.Top - 23);

            image.Mutate(a => a.DrawPolygon(new Pen(Color.Yellow, 1),
                new PointF(prediction.Rectangle.Left, prediction.Rectangle.Top),
                new PointF(prediction.Rectangle.Right, prediction.Rectangle.Top),
                new PointF(prediction.Rectangle.Right, prediction.Rectangle.Bottom),
                new PointF(prediction.Rectangle.Left, prediction.Rectangle.Bottom)
            ).DrawText($"{prediction.Label} ({score})",
                _font,
                Color.Yellow,
                new(x, y)
            ));
        }

        var ms = new MemoryStream();
        await image.SaveAsync(ms, new PngEncoder());
        ms.Position = 0;
        return ms;
    }

    public static async Task<string> MobileNet(Stream stream)
    {
        using var image = await Image.LoadAsync<Rgba32>(stream);

        using var scorer = new MobileNetScorer("Assets/mobilenetv2-7.onnx");

        var result = scorer.Predict(image);

        return result.Aggregate("", (current, r) => current + $"{r.Class} ({r.Score})\n")[..^1];
    }
}
