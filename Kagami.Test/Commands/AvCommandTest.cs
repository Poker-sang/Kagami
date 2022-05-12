using Kagami.Commands;

namespace Kagami.Test.Commands;

[TestClass]
public class AvCommandTest
{
    private const int aid = 553880774;

    private static async Task TestCommandAsync(string cmd)
    {
        var result = await Entry.ParseCommand(cmd);
        Assert.IsNotNull(result);
        Console.WriteLine(result.Build().ToString());
    }

    [TestMethod("AV号测试 - 大写")]
    public async Task TestAvUpperCaseCommandAsync() => await TestCommandAsync($"AV{aid}");

    [TestMethod("AV号测试 - 小写")]
    public async Task TestAvLowerCaseCommandAsync() => await TestCommandAsync($"av{aid}");

    [TestMethod("AV号测试 - 大小写")]
    public async Task TestAvUpperLowerCaseCommandAsync() => await TestCommandAsync($"Av{aid}");

    [TestMethod("AV号测试 - 小大写")]
    public async Task TestAvLowerUpperCaseCommandAsync() => await TestCommandAsync($"aV{aid}");

    [TestMethod("AV号测试 - 参数错写")]
    public async Task TestAvErrorCommandAsync()
    {
        try
        {
            var result = await Entry.ParseCommand("av553BB0774");
        }
        catch(Exception ex)
        {
            Assert.IsInstanceOfType(ex, typeof(ArgumentException));
            Assert.AreEqual(ex.Message, "参数类型不正确");
        }
    }
}