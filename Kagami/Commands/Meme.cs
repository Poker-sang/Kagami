using Kagami.ArgTypes;
using Kagami.Attributes;
using Konata.Core;
using Konata.Core.Events.Model;
using Konata.Core.Interfaces.Api;
using Konata.Core.Message;
using System.ComponentModel;
using static Kagami.Services.Meme;

namespace Kagami.Commands;

public static class Meme
{
    [KagamiCmdlet(nameof(Meme)), Description("弔图相关指令")]
    public static async Task<MessageBuilder> GetMeme(Bot bot, GroupMessageEvent group,
        [Description("弔图指令")] MemeCommands? commands = null,
        [Description("期数")] uint? intIssue = null)
    {
        // 期数的阿拉伯数字字符串
        if (commands is not null)
            switch (commands)
            {
                // 列出已有期数
                case MemeCommands.List:
                    return SendMemeList();
                // 更新图片
                case MemeCommands.Update:
                {
                    _ = await bot.SendGroupMessage(group.GroupUin, new MessageBuilder("正在获取弔图..."));

                    return intIssue?.ToString() is { } issue
                        ? await UpdateMemeAsync(issue, (int)intIssue)
                        : await UpdateMemeAsync();
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(commands));
            }
        // 发送图片
        else
            try
            {
                return await SendMemePicAsync(intIssue?.ToString() ?? await GetNewestIssue());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return new(e.Message);
            }
    }
}