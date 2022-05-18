using Kagami.ArgTypes;
using Kagami.Attributes;
using Konata.Core;
using Konata.Core.Events.Model;
using Konata.Core.Exceptions.Model;
using Konata.Core.Interfaces.Api;
using Konata.Core.Message;
using System.ComponentModel;

namespace Kagami.Commands;

public static class HighPermission
{
    [KagamiCmdlet(nameof(Mute), Permission = Konata.Core.Common.RoleType.Admin), Description("禁言成员（默认10分钟）")]
    public static async ValueTask<MessageBuilder> Mute(Bot bot, GroupMessageEvent group,
        [Description("成员")] At at,
        [Description("禁言时长")] uint minutes = 10)
    {
        try
        {
            return await bot.GroupMuteMember(group.GroupUin, at.Uin, minutes * 60)
                ? (new($"禁言 [{at.Uin}] {minutes}分钟"))
                : (new(StringResources.UnknownErrorMessage.RandomGet()));
        }
        catch (OperationFailedException e)
        {
            Console.WriteLine(e.Message);
            return new(StringResources.OperationFailedMessage.RandomGet());
        }
    }

    [KagamiCmdlet(nameof(Title), Permission = Konata.Core.Common.RoleType.Owner), Description("为成员设置头衔")]
    public static async ValueTask<MessageBuilder> Title(Bot bot, GroupMessageEvent group,
        [Description("成员")] At at,
        [Description("头衔")] string title)
    {
        try
        {
            return await bot.GroupSetSpecialTitle(group.GroupUin, at.Uin, title, uint.MaxValue)
                ? (new($"为 [{at.Uin}] 设置头衔"))
                : (new(StringResources.UnknownErrorMessage.RandomGet()));
        }
        catch (OperationFailedException e)
        {
            Console.WriteLine($"{e.Message} ({e.HResult})");
            return new(StringResources.OperationFailedMessage.RandomGet());
        }
    }
}
