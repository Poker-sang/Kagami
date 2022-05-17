using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kagami.Services;

namespace Kagami.Services.Tests;

[TestClass()]
public class HelpTests
{
    [TestMethod("测试生成帮助HTML")]
    public void GenerateHtmlTest()
    {
        string html = Help.GenerateHtml();
        Console.Error.WriteLine(html);
        Assert.IsNotNull(html);
        Assert.IsFalse(string.IsNullOrWhiteSpace(html));
    }

    [TestMethod("测试生成帮助图片")]
    public async Task GenerateImageTestAsync()
    {
        var bytes = await Help.GenerateImageAsync();
        Assert.IsNotNull(bytes);
        await using var fs = File.Create("./test.png");
        await fs.WriteAsync(bytes);
        await fs.FlushAsync();
    }

    [TestMethod("测试不使用缓存生成帮助图片")]
    public async Task GenerateImageWithoutCacheTestAsync()
    {
        var bytes = await Help.GenerateImageWithoutCacheAsync();
        Assert.IsNotNull(bytes);
        await using var fs = File.Create("./test1.png");
        await fs.WriteAsync(bytes);
        await fs.FlushAsync();
    }
}