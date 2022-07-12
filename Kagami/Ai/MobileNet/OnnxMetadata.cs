using Microsoft.ML.OnnxRuntime;

namespace Kagami.Ai.MobileNet;

public class OnnxMetadata
{
    public string Name { get; }
    public int Batch { get; }
    public int Channel { get; }
    public int Width { get; }
    public int Height { get; }

    public OnnxMetadata(KeyValuePair<string, NodeMetadata> metadata)
    {
        Name = metadata.Key;
        Batch = metadata.Value.Dimensions[0];
        Channel = metadata.Value.Dimensions[1];
        Width = metadata.Value.Dimensions[2];
        Height = metadata.Value.Dimensions[3];
    }
}
