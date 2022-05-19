namespace Kagami.Core;

internal static class StringResources
{
    public static readonly string[] RollMessage = {
        "嗯让我想想ww......果然还是{0}好！",
        "emmm我想选{0}吧x",
        "要不还是选{0}呢？",
        "就你了！{0}！",
    };
    public static readonly string[] ArgumentErrorMessage = { "参数不对哦", "请再检查一下参数", "没听懂欸", "嗯？", "好笨，参数错了哦" };
    public static readonly string[] UnknownErrorMessage = { "咱也不知道出了什么问题", "呜呜失败了" };
    public static readonly string[] OperationFailedMessage = { "操作失败了..." };
    public static readonly string[] ProcessingMessage = { "正在执行中x", "请稍后噢", "很快就完成叻", "开始获取~..." };
    public static readonly string[] PokeMessage = { "你再戳", "rua", "不许戳", "戳♥死♥我", "呜呜", "别戳了别戳了", "啊啊啊", "。", "？", "！", "喵", "呜", "您？" };

    public static T RandomGet<T>(this IReadOnlyList<T> array)
        => /*int.TryParse(await Services.Dice.RollAsync(array.Count), out int index) ? array[index] :*/ array[Random.Shared.Next(array.Count)];

}
