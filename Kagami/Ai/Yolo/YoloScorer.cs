﻿using Kagami.Ai.Yolo.Models.Abstract;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Collections.Concurrent;

namespace Kagami.Ai.Yolo;

/// <summary>
/// Yolov5 scorer.
/// </summary>
public class YoloScorer<T> : IDisposable where T : YoloModel
{
    private readonly T _model;

    private readonly InferenceSession _inferenceSession = null!;

    /// <summary>
    /// Outputs value between 0 and 1.
    /// </summary>
    private static float Sigmoid(float value) => 1 / (1 + (float)Math.Exp(-value));

    /// <summary>
    /// Converts xywh bbox format to xyxy.
    /// </summary>
    private float[] Xywh2Xyxy(float[] source)
    {
        var result = new float[4];

        result[0] = source[0] - (source[2] / 2f);
        result[1] = source[1] - (source[3] / 2f);
        result[2] = source[0] + (source[2] / 2f);
        result[3] = source[1] + (source[3] / 2f);

        return result;
    }

    /// <summary>
    /// Returns value clamped to the inclusive range of min and max.
    /// </summary>
    public float Clamp(float value, float min, float max) => (value < min) ? min : (value > max) ? max : value;

    /// <summary>
    /// Runs inference session.
    /// </summary>
    private DenseTensor<float>[] Inference(Image<Rgba32> image)
    {
        if (image.Width != _model.Width || image.Height != _model.Height)
            image.Mutate(x => x.Resize(_model.Width, _model.Height)); // fit image size to specified input size

        var inputs = new[] // add image as onnx input
        {
            NamedOnnxValue.CreateFromTensor("images", image.ExtractPixels())
        };

        var result = _inferenceSession.Run(inputs); // run inference

        // add outputs for processing

        return _model.Outputs.Select(item => (DenseTensor<float>)result.First(x => x.Name == item).Value).ToArray();
    }

    /// <summary>
    /// Parses net output (detect) to predictions.
    /// </summary>
    private List<YoloPrediction> ParseDetect(DenseTensor<float> output, (int Width, int Height) info)
    {
        var result = new ConcurrentBag<YoloPrediction>();

        var (xGain, yGain) = (_model.Width / (float)info.Width, _model.Height / (float)info.Height); // x, y gains
        var gain = Math.Min(xGain, yGain); // gain = resized / original

        var (xPad, yPad) = ((_model.Width - (info.Width * xGain)) / 2, (_model.Height - (info.Height * yGain)) / 2); // left, right pads

        _ = Parallel.For(0, (int)output.Length / _model.Dimensions, i =>
        {
            if (output[0, i, 4] <= _model.Confidence)
                return; // skip low obj_conf results

            _ = Parallel.For(5, _model.Dimensions, j => output[0, i, j] *= output[0, i, 4]);

            _ = Parallel.For(5, _model.Dimensions, k =>
            {
                if (output[0, i, k] <= _model.MulConfidence)
                    return; // skip low mul_conf results

                var xMin = (output[0, i, 0] - (output[0, i, 2] / 2) - xPad) / xGain; // unpad bbox tlx to original
                var yMin = (output[0, i, 1] - (output[0, i, 3] / 2) - yPad) / yGain; // unpad bbox tly to original
                var xMax = (output[0, i, 0] + (output[0, i, 2] / 2) - xPad) / xGain; // unpad bbox brx to original
                var yMax = (output[0, i, 1] + (output[0, i, 3] / 2) - yPad) / yGain; // unpad bbox bry to original

                xMin = Clamp(xMin, 0, info.Width - 0); // clip bbox tlx to boundaries
                yMin = Clamp(yMin, 0, info.Height - 0); // clip bbox tly to boundaries
                xMax = Clamp(xMax, 0, info.Width - 1); // clip bbox brx to boundaries
                yMax = Clamp(yMax, 0, info.Height - 1); // clip bbox bry to boundaries

                var label = _model.Labels[k - 5];

                var prediction = new YoloPrediction(label, output[0, i, k], new(xMin, yMin, xMax - xMin, yMax - yMin));

                result.Add(prediction);
            });
        });

        return result.ToList();
    }

    /// <summary>
    /// Parses net outputs (sigmoid) to predictions.
    /// </summary>
    private List<YoloPrediction> ParseSigmoid(DenseTensor<float>[] output, (int Width, int Height) info)
    {
        var result = new ConcurrentBag<YoloPrediction>();

        var (xGain, yGain) = (_model.Width / (float)info.Width, _model.Height / (float)info.Height); // x, y gains

        var (xPad, yPad) = ((_model.Width - (info.Width * xGain)) / 2, (_model.Height - (info.Height * yGain)) / 2); // left, right pads

        _ = Parallel.For(0, output.Length, i => // iterate model outputs
        {
            var shapes = _model.Shapes[i]; // shapes per output

            _ = Parallel.For(0, _model.Anchors[0].Length, a => // iterate anchors
                Parallel.For(0, shapes, y => // iterate shapes (rows)
                    Parallel.For(0, shapes, x => // iterate shapes (columns)
                    {
                        var offset = ((shapes * shapes * a) + (shapes * y) + x) * _model.Dimensions;

                        var buffer = output[i].Skip(offset).Take(_model.Dimensions).Select(Sigmoid).ToArray();

                        if (buffer[4] <= _model.Confidence)
                            return; // skip low obj_conf results

                        var scores = buffer.Skip(5).Select(b => b * buffer[4]).ToList(); // mul_conf = obj_conf * cls_conf

                        var mulConfidence = scores.Max(); // max confidence score

                        if (mulConfidence <= _model.MulConfidence)
                            return; // skip low mul_conf results

                        var rawX = ((buffer[0] * 2) - 0.5f + x) * _model.Strides[i]; // predicted bbox x (center)
                        var rawY = ((buffer[1] * 2) - 0.5f + y) * _model.Strides[i]; // predicted bbox y (center)

                        var rawW = (float)Math.Pow(buffer[2] * 2, 2) * _model.Anchors[i][a][0]; // predicted bbox w
                        var rawH = (float)Math.Pow(buffer[3] * 2, 2) * _model.Anchors[i][a][1]; // predicted bbox h

                        var xyxy = Xywh2Xyxy(new[] { rawX, rawY, rawW, rawH });

                        var xMin = Clamp((xyxy[0] - xPad) / xGain, 0, info.Width - 0); // unpad, clip tlx
                        var yMin = Clamp((xyxy[1] - yPad) / yGain, 0, info.Height - 0); // unpad, clip tly
                        var xMax = Clamp((xyxy[2] - xPad) / xGain, 0, info.Width - 1); // unpad, clip brx
                        var yMax = Clamp((xyxy[3] - yPad) / yGain, 0, info.Height - 1); // unpad, clip bry

                        var label = _model.Labels[scores.IndexOf(mulConfidence)];

                        var prediction = new YoloPrediction(label, mulConfidence, new(xMin, yMin, xMax - xMin, yMax - yMin));

                        result.Add(prediction);
                    })));
        });

        return result.ToList();
    }

    /// <summary>
    /// Parses net outputs (sigmoid or detect layer) to predictions.
    /// </summary>
    private List<YoloPrediction> ParseOutput(DenseTensor<float>[] output, (int Width, int Height) info) => _model.UseDetect ? ParseDetect(output[0], info) : ParseSigmoid(output, info);

    /// <summary>
    /// Removes overlapped duplicates (nms).
    /// </summary>
    private List<YoloPrediction> Suppress(List<YoloPrediction> items)
    {
        var result = new List<YoloPrediction>(items);

        foreach (var item in items) // iterate every prediction
        {
            foreach (var current in result.ToList().Where(current => current != item)) // make a copy for each iteration
            {
                var (rect1, rect2) = (item.Rectangle, current.Rectangle);

                var intersection = RectangleF.Intersect(rect1, rect2);

                var intArea = intersection.Area(); // intersection area
                var unionArea = rect1.Area() + rect2.Area() - intArea; // union area
                var overlap = intArea / unionArea; // overlap ratio

                if (overlap >= _model.Overlap)
                    if (item.Score >= current.Score)
                        _ = result.Remove(current);
            }
        }

        return result;
    }

    /// <summary>
    /// Runs object detection.
    /// </summary>
    public List<YoloPrediction> Predict(Image<Rgba32> image) => Suppress(ParseOutput(Inference(image.Clone()), (image.Width, image.Height)));

    /// <summary>
    /// Creates new instance of YoloScorer.
    /// </summary>
    public YoloScorer() => _model = Activator.CreateInstance<T>();

    /// <summary>
    /// Creates new instance of YoloScorer with weights path and options.
    /// </summary>
    public YoloScorer(string weights, SessionOptions? opts = null) : this() => _inferenceSession = new(File.ReadAllBytes(weights), opts ?? new SessionOptions());

    /// <summary>
    /// Creates new instance of YoloScorer with weights stream and options.
    /// </summary>
    public YoloScorer(Stream weights, SessionOptions? opts = null) : this()
    {
        using var reader = new BinaryReader(weights);
        _inferenceSession = new(reader.ReadBytes((int)weights.Length), opts ?? new SessionOptions());
    }

    /// <summary>
    /// Creates new instance of YoloScorer with weights bytes and options.
    /// </summary>
    public YoloScorer(byte[] weights, SessionOptions? opts = null) : this() => _inferenceSession = new(weights, opts ?? new SessionOptions());

    /// <summary>
    /// Disposes YoloScorer instance.
    /// </summary>
    public void Dispose()
    {
        _inferenceSession.Dispose();
        GC.SuppressFinalize(this);
    }
}
