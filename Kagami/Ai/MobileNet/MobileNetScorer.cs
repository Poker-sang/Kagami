using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Kagami.Ai.MobileNet;

public partial class MobileNetScorer : IDisposable
{
    private readonly InferenceSession _inferenceSession;

    /// <summary>
    /// Runs inference session.
    /// </summary>
    public DenseTensor<float> Inference(Image<Rgba32> image)
    {
        if (_inferenceSession.InputMetadata is not { Count: > 0 })
            throw new();

        var inputMetadata = new OnnxMetadata(_inferenceSession.InputMetadata.First());

        if (image.Width != inputMetadata.Width || image.Height != inputMetadata.Height)
            image.Mutate(x => x.Resize(inputMetadata.Width, inputMetadata.Height));

        var inputs = new List<NamedOnnxValue> // 添加输入
        {
            NamedOnnxValue.CreateFromTensor(inputMetadata.Name, image.ExtractPixels())
        };

        var result = _inferenceSession.Run(inputs); // 运行检测

        return (DenseTensor<float>)result.First().Value;
    }

    /// <summary>
    /// Runs object detection.
    /// </summary>
    public List<(string Class, float Score)> Predict(Image<Rgba32> image)
    {
        var scores = Inference(image).ToList();
        var result = new List<(string Class, float Score)>();

        float score;
        do
        {
            score = scores.Max();
            var index = scores.IndexOf(score);
            scores[index] = 0;
            result.Add(new(_classes[index], score));
        } while (score > 10);
        if (result.Count > 1)
            result.RemoveAt(result.Count - 1);
        return result;
    }

    public MobileNetScorer(string weights, SessionOptions? opts = null) => _inferenceSession =
        new(File.ReadAllBytes(weights), opts ?? new SessionOptions());

    public MobileNetScorer(Stream weights, SessionOptions? opts = null)
    {
        using var reader = new BinaryReader(weights);
        _inferenceSession = new(reader.ReadBytes((int)weights.Length), opts ?? new SessionOptions());
    }

    public MobileNetScorer(byte[] weights, SessionOptions? opts = null) =>
        _inferenceSession = new(weights, opts ?? new SessionOptions());

    public void Dispose()
    {
        _inferenceSession.Dispose();
        GC.SuppressFinalize(this);
    }
}
