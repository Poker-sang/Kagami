using Konata.Core.Message.Model;

namespace Kagami.Test;

[TestClass]
public class CommandTest
{
    [TestMethod("BV号测试")]
    public async Task TestBvCommandAsync()
    {
        var result = await Function.Commands.Bv(TextChain.Create("bv BV1Fv4y1T7Cc"));
        Console.WriteLine(result.Build().ToString());
    }
    [TestMethod("AV号测试")]
    public async Task TestAvCommandAsync()
    {
        var result = await Function.Commands.Av(TextChain.Create("av av553880774"));
        Console.WriteLine(result.Build().ToString());
    }

    [TestMethod("AC号测试")]
    public async Task TestAcCommandAsync()
    {
        var result = await Function.Commands.Ac(TextChain.Create("ac ac34786008"));
        Console.WriteLine(result.Build().ToString());
    }
}