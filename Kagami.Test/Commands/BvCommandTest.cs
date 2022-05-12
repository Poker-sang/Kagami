using Kagami.Commands;
using Konata.Core.Message.Model;

namespace Kagami.Test.Commands;

[TestClass]
public class BvCommandTest
{
    private const string bvid = "1Fv4y1T7Cc";

    private static async Task TestCommandAsync(string cmd)
    {
        try
        {
            var result = await Entry.ParseCommand(cmd);
        }
        catch (Exception ex)
        {
            Assert.IsInstanceOfType(ex, typeof(ArgumentException));
            Assert.AreEqual(ex.Message, "参数类型不正确");
        }
    }

    [TestMethod("BV号测试 - 大写")]
    public async Task TestBvUpperCaseCommandAsync()
    {
        var result = await Entry.ParseCommand($"BV{bvid}");
        Assert.IsNotNull(result);
        Console.WriteLine(result.Build().ToString());
    }

    [TestMethod("BV号测试 - 小写")]
    public async Task TestBvLowerCaseCommandAsync() => await TestCommandAsync($"BV{bvid}");

    [TestMethod("BV号测试 - 大小写")]
    public async Task TestBvUpperLowerCaseCommandAsync() => await TestCommandAsync($"BV{bvid}");

    [TestMethod("BV号测试 - 小大写")]
    public async Task TestBvLowerUpperCaseCommandAsync() => await TestCommandAsync($"BV{bvid}");

    [TestMethod("BV号测试 - 参数错写")]
    public async Task TestBvErrorCommandAsync() => await TestCommandAsync("BV1Fv4y21T7Cc");
}