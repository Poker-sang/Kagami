using Kagami.Utils;
using Konata.Core.Common;
using Konata.Core.Exceptions.Model;
using Konata.Core.Interfaces.Api;
using Konata.Core.Message;
using Konata.Core.Message.Model;

namespace Kagami.Commands;

/// <summary>
/// <inheritdoc/>
/// </summary>
public sealed class MemberCommand : IKagamiCommand
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public string Command { get; } = "member";

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public string Description { get; } = "获取成员信息";

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public (Type, string)[] Arguments { get; } = new[] {
        (typeof(ArgTypes.At), "成员"),
    };

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public async Task<MessageBuilder> InvokeAsync(Konata.Core.Bot? bot, Konata.Core.Events.Model.GroupMessageEvent? group, object[] args)
    {
        Assert.IsNotNull<ArgumentException>(bot, "内部错误 - bot不能为空", nameof(bot));
        Assert.IsNotNull<ArgumentException>(group, "内部错误 - group不能为空", nameof(group));

        // Get at
        if (args[0] is not ArgTypes.At at)
            return new(await StringResources.ArgumentErrorMessage.RandomGetAsync());

        // Get group info
        var memberInfo = await bot.GetGroupMemberInfo(group.GroupUin, at.Uin, true);
        if (memberInfo is null)
            return new("没有找到这个人x");

        return new MessageBuilder($"[{memberInfo.NickName}]")
            .TextLine($"群名片：{memberInfo.Name}")
            .TextLine($"加入时间：{memberInfo.JoinTime}")
            .TextLine($"类别：{memberInfo.Role switch
            {
                RoleType.Member => "成员",
                RoleType.Admin => "管理员",
                RoleType.Owner => "群主",
                _ => throw new ArgumentOutOfRangeException(nameof(memberInfo.Role), "未知的类别")
            }}")
            .TextLine($"等级：{memberInfo.Level}")
            .TextLine($"头衔：{memberInfo.SpecialTitle}");
    }
}
