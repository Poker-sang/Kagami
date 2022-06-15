using Kagami.ArgTypes;
using Kagami.Attributes;
using Kagami.Enums;
using Konata.Core.Message;
using System.ComponentModel;
using static Kagami.Services.Meme;

namespace Kagami.Commands;

public static class Meme
{
    [Obsolete("容易被封禁")]
    [Cmdlet(nameof(Meme), ParameterType = ParameterType.Reverse), Description("弔图其他指令")]
    public static async Task<MessageBuilder> MemeCommand(
        [Description("期数")] uint? intIssue = null,
        [Description("弔图指令")] MemeOption commands = MemeOption.Send)
    {
        switch (commands)
        {
            // 列出已有期数
            case MemeOption.List:
                return SendMemeList();
            // 更新图片
            case MemeOption.Update:
            {
                return await (intIssue?.ToString() is { } issue
                    ? UpdateMemeAsync(issue, (int)intIssue)
                    : UpdateMemeAsync());
            }
            // 发送图片
            case MemeOption.Send:
                try
                {
                    return SendMemePicAsync(intIssue?.ToString() ?? GetNewestIssue().GetAwaiter().GetResult()).GetAwaiter().GetResult();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return new(e.Message);
                }
            default:
                throw new NotSupportedException("不支持的命令：" + commands);
        }
    }
}
