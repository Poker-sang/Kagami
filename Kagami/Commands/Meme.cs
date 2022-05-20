using Kagami.ArgTypes;
using Kagami.Attributes;
using Konata.Core.Message;
using System.ComponentModel;
using static Kagami.Services.Meme;

namespace Kagami.Commands;

public static class Meme
{
    [Cmdlet(nameof(Meme)), Description("弔图其他指令")]
    public static async Task<MessageBuilder> MemeCommand(
        [Description("弔图指令")] MemeOption commands,
        [Description("期数")] uint? intIssue = null)
    {
        switch (commands)
        {
            // 列出已有期数
            case MemeOption.List:
                return SendMemeList();
            // 更新图片
            case MemeOption.Update:
            {
                return intIssue?.ToString() is { } issue
                    ? await UpdateMemeAsync(issue, (int)intIssue)
                    : await UpdateMemeAsync();
            }
            default:
                throw new NotSupportedException($"不支持的命令 {commands}");
        }
    }

    [Cmdlet(nameof(Meme)), Description("发送弔图")]
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
