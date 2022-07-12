using Kagami.Ai.Yolo.Models.Abstract;

namespace Kagami.Ai.Yolo.Models;

public class YoloCocoP5Model : YoloModel
{
    public override int Width { get; set; } = 640;
    public override int Height { get; set; } = 640;
    public override int Depth { get; set; } = 3;

    public override int Dimensions { get; set; } = 85;

    public override int[] Strides { get; set; } = { 8, 16, 32 };

    public override int[][][] Anchors { get; set; } = {
        new[] { new[] { 010, 13 }, new[] { 016, 030 }, new[] { 033, 023 } },
        new[] { new[] { 030, 61 }, new[] { 062, 045 }, new[] { 059, 119 } },
        new[] { new[] { 116, 90 }, new[] { 156, 198 }, new[] { 373, 326 } }
    };

    public override int[] Shapes { get; set; } = { 80, 40, 20 };

    public override float Confidence { get; set; } = 0.20f;
    public override float MulConfidence { get; set; } = 0.25f;
    public override float Overlap { get; set; } = 0.45f;

    public override string[] Outputs { get; set; } = { "output" };

    public override string[] Labels { get; set; } =
    {
        "person",
        "bicycle",
        "car",
        "motorcycle",
        "airplane",
        "bus",
        "train",
        "truck",
        "boat",
        "traffic light",
        "fire hydrant",
        "stop sign",
        "parking meter",
        "bench",
        "bird",
        "cat",
        "dog",
        "horse",
        "sheep",
        "cow",
        "elephant",
        "bear",
        "zebra",
        "giraffe",
        "backpack",
        "umbrella",
        "handbag",
        "tie",
        "suitcase",
        "frisbee",
        "skis",
        "snowboard",
        "sports ball",
        "kite",
        "baseball bat",
        "baseball glove",
        "skateboard",
        "surfboard",
        "tennis racket",
        "bottle",
        "wine glass",
        "cup",
        "fork",
        "knife",
        "spoon",
        "bowl",
        "banana",
        "apple",
        "sandwich",
        "orange",
        "broccoli",
        "carrot",
        "hot dog",
        "pizza",
        "donut",
        "cake",
        "chair",
        "couch",
        "potted plant",
        "bed",
        "dining table",
        "toilet",
        "tv",
        "laptop",
        "mouse",
        "remote",
        "keyboard",
        "cell phone",
        "microwave",
        "oven",
        "toaster",
        "sink",
        "refrigerator",
        "book",
        "clock",
        "vase",
        "scissors",
        "teddy bear",
        "hair drier",
        "toothbrush"
    };

    public override bool UseDetect { get; set; } = true;
}
