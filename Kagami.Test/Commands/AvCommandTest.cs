// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Kagami.Test.Commands;

[TestClass]
public class AvCommandTest
{
    private const int Aid = 553880774;

    private static async Task TestCommandAsync(string cmd)
    {
        Konata.Core.Message.MessageBuilder? result = await Entry.ParseCommand(cmd, null!, null!);
        Assert.IsNotNull(result);
        Console.WriteLine(result.Build().ToString());
    }

    [TestMethod("AV号测试 - 大写")]
    public async Task TestAvUpperCaseCommandAsync() => await TestCommandAsync($"AV{Aid}");

    [TestMethod("AV号测试 - 小写")]
    public async Task TestAvLowerCaseCommandAsync() => await TestCommandAsync($"av{Aid}");

    [TestMethod("AV号测试 - 大小写")]
    public async Task TestAvUpperLowerCaseCommandAsync() => await TestCommandAsync($"Av{Aid}");

    [TestMethod("AV号测试 - 小大写")]
    public async Task TestAvLowerUpperCaseCommandAsync() => await TestCommandAsync($"aV{Aid}");

    [TestMethod("AV号测试 - 参数错写")]
    public async Task TestAvErrorCommandAsync()
    {
        try
        {
            Konata.Core.Message.MessageBuilder? result = await Entry.ParseCommand("av553BB0774", null!, null!);
        }
        catch (Exception ex)
        {
            Assert.IsInstanceOfType(ex, typeof(ArgumentException));
            Assert.AreEqual(ex.Message, "参数类型不正确");
        }
    }
}
