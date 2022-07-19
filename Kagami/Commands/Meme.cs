using Kagami.ArgTypes;
using Kagami.Core;
using Kagami.UsedTypes;
using Konata.Core;
using Konata.Core.Message;
using Konata.Core.Message.Model;
using System.ComponentModel;
using static Kagami.Services.Meme;

namespace Kagami.Commands;

public static class Meme
{
    // [Obsolete("容易被封禁")]
    [Cmdlet(nameof(Meme), "弔图", ParameterType = ParameterType.Reverse), Description("弔图其他指令")]
    public static async Task<MessageBuilder[]> MemeCommand(
        Bot bot,
        [Description("期数")] uint? intIssue = null,
        [Description("弔图指令")] MemeOption commands = MemeOption.Send)
    {
        switch (commands)
        {
            // 列出已有期数
            case MemeOption.List:
                return new MessageBuilder[] { new(GetMemeList()) };
            // 更新图片
            case MemeOption.Update:
            {
                return new MessageBuilder[] { new
                    (await (intIssue?.ToString() is { } issue
                    ? UpdateMemeAsync(issue, (int)intIssue)
                    : UpdateMemeAsync()))};
            }
            // 发送图片
            case MemeOption.Send:
                try
                {
                    var (result, image) = GetMemePicAsync(intIssue?.ToString() ?? GetNewestIssue().GetAwaiter().GetResult()).GetAwaiter().GetResult();
                    return new[] { image is { }
                        ? new MessageBuilder(result).Add(ImageChain.CreateFromFile(image))
                        : new(result)};
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return new MessageBuilder[] { new(e.Message) };
                }
            // 发送整期沙雕图
            case MemeOption.All:
            {
                var (result, images) = GetAllMeme(intIssue?.ToString() ?? await GetNewestIssue());
                if (images is { })
                {
                    await Task.Yield();
                    var multiMsgChains = new List<MultiMsgChain>();
                    for (var i = 0; i < images.Length; ++i)
                    {
                        if (i % MaxImages is 0)
                            multiMsgChains.Add(new() { ((bot.Uin, bot.Name), $"{result}\n{(i / MaxImages) + 1}") });
                        _ = multiMsgChains[^1].AddMessage(bot.Uin, bot.Name, ImageChain.CreateFromFile(images[i]));
                    }

                    //var temp = new MultiMsgChain();
                    //foreach (var multiMsgChain in multiMsgChains)
                    //    _ = temp.AddMessage(bot.Uin, bot.Name, multiMsgChain);
                    //return new[] { new MessageBuilder(temp) };
                    return multiMsgChains.Select(mmc => new MessageBuilder(mmc)).ToArray();
                }
                else
                    return new MessageBuilder[] { new(result) };
            }
            default:
                throw new NotSupportedException("不支持的命令：" + commands);
        }
    }

    private const int MaxImages = StringResources.MaxMultiMessages - 1;
}
