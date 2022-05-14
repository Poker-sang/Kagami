using Kagami.Services;

namespace Kagami.Test.Services;

[TestClass]
public class MemeTest
{
    [TestMethod()]
    public async Task TestGetMemeImageSourcesAsync()
    {
        var list = await Meme.GetMemeImageSourcesAsync("二百四十四");
        Console.WriteLine("Images: [\n  ");
        Console.WriteLine(string.Join(",\n  ", list));
        Console.WriteLine("]");
    }
}