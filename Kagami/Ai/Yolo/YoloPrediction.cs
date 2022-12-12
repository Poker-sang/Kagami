
using SixLabors.ImageSharp;

namespace Kagami.Ai.Yolo;

/// <summary>
/// Object prediction.
/// </summary>
public record YoloPrediction(string Label, float Score, RectangleF Rectangle);
