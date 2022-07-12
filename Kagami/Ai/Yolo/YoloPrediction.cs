
using SixLabors.ImageSharp;

namespace Kagami.Ai.Yolo;

/// <summary>
/// Object prediction.
/// </summary>
public class YoloPrediction
{
    public string Label { get; set; }
    public RectangleF Rectangle { get; set; }
    public float Score { get; set; }

    public YoloPrediction(string label, float confidence) : this(label) => Score = confidence;

    public YoloPrediction(string label) => Label = label;
}
