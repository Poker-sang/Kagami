namespace Kagami.Test.Services;

[TestClass]
public class AiTest
{
    [TestMethod("Yolov5验证")]
    public async Task TestYolo()
    {
        await using var download = File.OpenRead(@".\Assets\test.jpg");
        await using var stream = await Kagami.Services.ArtificialIntelligence.Yolo(download);
        var buffer = new byte[stream.Length];
        _ = await stream.ReadAsync(buffer);
        await File.WriteAllBytesAsync(@".\Assets\result.jpg", buffer);
        Console.WriteLine("Yolo v5 ran successfully.");
    }
}
