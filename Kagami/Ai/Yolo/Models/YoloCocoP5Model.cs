﻿using Kagami.Ai.Yolo.Models.Abstract;

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

    public override List<YoloLabel> Labels { get; set; } = new()
    {
        new(1, "person"),
        new(2, "bicycle"),
        new(3, "car"),
        new(4, "motorcycle"),
        new(5, "airplane"),
        new(6, "bus"),
        new(7, "train"),
        new(8, "truck"),
        new(9, "boat"),
        new(10, "traffic light"),
        new(11, "fire hydrant"),
        new(12, "stop sign"),
        new(13, "parking meter"),
        new(14, "bench"),
        new(15, "bird"),
        new(16, "cat"),
        new(17, "dog"),
        new(18, "horse"),
        new(19, "sheep"),
        new(20, "cow"),
        new(21, "elephant"),
        new(22, "bear"),
        new(23, "zebra"),
        new(24, "giraffe"),
        new(25, "backpack"),
        new(26, "umbrella"),
        new(27, "handbag"),
        new(28, "tie"),
        new(29, "suitcase"),
        new(30, "frisbee"),
        new(31, "skis"),
        new(32, "snowboard"),
        new(33, "sports ball"),
        new(34, "kite"),
        new(35, "baseball bat"),
        new(36, "baseball glove"),
        new(37, "skateboard"),
        new(38, "surfboard"),
        new(39, "tennis racket"),
        new(40, "bottle"),
        new(41, "wine glass"),
        new(42, "cup"),
        new(43, "fork"),
        new(44, "knife"),
        new(45, "spoon"),
        new(46, "bowl"),
        new(47, "banana"),
        new(48, "apple"),
        new(49, "sandwich"),
        new(50, "orange"),
        new(51, "broccoli"),
        new(52, "carrot"),
        new(53, "hot dog"),
        new(54, "pizza"),
        new(55, "donut"),
        new(56, "cake"),
        new(57, "chair"),
        new(58, "couch"),
        new(59, "potted plant"),
        new(60, "bed"),
        new(61, "dining table"),
        new(62, "toilet"),
        new(63, "tv"),
        new(64, "laptop"),
        new(65, "mouse"),
        new(66, "remote"),
        new(67, "keyboard"),
        new(68, "cell phone"),
        new(69, "microwave"),
        new(70, "oven"),
        new(71, "toaster"),
        new(72, "sink"),
        new(73, "refrigerator"),
        new(74, "book"),
        new(75, "clock"),
        new(76, "vase"),
        new(77, "scissors"),
        new(78, "teddy bear"),
        new(79, "hair drier"),
        new(80, "toothbrush")
    };

    public override bool UseDetect { get; set; } = true;
}
