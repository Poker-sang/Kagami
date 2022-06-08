using Kagami.ArgTypes;
// using Kagami.Attributes;
// using Kagami.Enums;
using Konata.Core.Message;
using System.ComponentModel;

namespace Kagami.Commands;

public static class Novel
{

    [Obsolete("容易被封禁")]
    // [Cmdlet(nameof(Novel), ParameterType = ParameterType.Reverse), Description("续写小说")]
    public static async Task<MessageBuilder> WriteNovel(
        [Description("内容")] string content,
        [Description("题目")] string title = "",
        [Description("模式")] NovelDream mode = NovelDream.Bot0)
        => new(await Services.Novel.WriteNovel(mode, title, content));
}
