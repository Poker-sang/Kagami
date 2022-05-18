using System.ComponentModel;

using Kagami.ArgTypes;
using Kagami.Attributes;

using Konata.Core;
using Konata.Core.Events.Model;
using Konata.Core.Interfaces.Api;
using Konata.Core.Message;

using static Kagami.Services.Meme;

namespace Kagami.Commands;

public static class Meme
{
    [KagamiCmdlet(nameof(Meme)), Description("弔图相关指令")]
    public static async Task<MessageBuilder> GetMeme(
        [Description("弔图指令")] MemeCommand commands,
        [Description("期数")] uint? intIssue = null)
    {
        switch (commands)
        {
            // 列出已有期数
            case MemeCommand.List:
                return SendMemeList();
            // 更新图片
            case MemeCommand.Update:
                {
                    return intIssue?.ToString() is { } issue
                        ? await UpdateMemeAsync(issue, (int)intIssue)
                        : await UpdateMemeAsync();
                }
            default:
                throw new NotSupportedException($"不支持的命令 {commands}");
        }
    }

    [KagamiCmdlet(nameof(Meme)), Description("弔图相关指令")]
    public static async ValueTask<MessageBuilder> GetMeme(
        [Description("期数")] uint? intIssue = null)
    {
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
