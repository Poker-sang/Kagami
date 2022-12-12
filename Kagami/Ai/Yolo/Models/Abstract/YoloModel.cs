namespace Kagami.Ai.Yolo.Models.Abstract;

/// <summary>
/// Model descriptor.
/// </summary>
public record YoloModel(
    int Width,
    int Height,
    int Depth,

    int Dimensions,

    int[] Strides,
    int[][][] Anchors,
    int[] Shapes,

    float Confidence,
    float MulConfidence,
    float Overlap,

    string[] Outputs,
    string[] Labels,
    bool UseDetect
);
