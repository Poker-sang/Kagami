// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Kagami.Test.Commands;

[TestClass]
public class BvCommandTest
{
    private const string Bvid = "1Fv4y1T7Cc";

    private static async Task TestCommandAsync(string cmd)
    {
        try
        {
            Konata.Core.Message.MessageBuilder? result = await CommandParser.ParseRawCommand(cmd, null!, null!);
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
        Konata.Core.Message.MessageBuilder? result = await CommandParser.ParseRawCommand($"BV{Bvid}", null!, null!);
        Assert.IsNotNull(result);
        Console.WriteLine(result.Build().ToString());
    }

    [TestMethod("BV号测试 - 小写")]
    public async Task TestBvLowerCaseCommandAsync() => await TestCommandAsync($"BV{Bvid}");

    [TestMethod("BV号测试 - 大小写")]
    public async Task TestBvUpperLowerCaseCommandAsync() => await TestCommandAsync($"BV{Bvid}");

    [TestMethod("BV号测试 - 小大写")]
    public async Task TestBvLowerUpperCaseCommandAsync() => await TestCommandAsync($"BV{Bvid}");

    [TestMethod("BV号测试 - 参数错写")]
    public async Task TestBvErrorCommandAsync() => await TestCommandAsync("BV1Fv4y21T7Cc");
}
