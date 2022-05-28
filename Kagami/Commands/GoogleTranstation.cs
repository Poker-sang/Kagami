using Kagami.ArgTypes;
using Kagami.Attributes;
using Kagami.Enums;
using Kagami.Services;
using Konata.Core.Message;
using System.ComponentModel;

namespace Kagami.Commands;
public static class GoogleTranslation
{
    [Cmdlet(nameof(Trans), ParameterType = ParameterType.Reverse), Description("谷歌翻译")]
    public static async ValueTask<MessageBuilder> Trans(
        [Description("翻译内容")] string raw,
        [Description("目标语言（默认英文）")] Languages target = Languages.En,
        [Description("原始语言（默认自动判断）")] Languages from = Languages.Auto) => new(await raw.TranslateText(from, target));
}
