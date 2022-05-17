using System.ComponentModel;
using Kagami.ArgTypes;
using Kagami.Attributes;
using Konata.Core;
using Konata.Core.Events.Model;
using Konata.Core.Exceptions.Model;
using Konata.Core.Interfaces.Api;
using Konata.Core.Message;

namespace Kagami.Commands;

public static class HighPermission
{
    [KagamiCmdlet(nameof(Mute), Permission = Konata.Core.Common.RoleType.Admin), Description("禁言成员（默认10分钟）")]
    public static async ValueTask<MessageBuilder> Mute(Bot bot, GroupMessageEvent group,
        [Description("成员")] At at,
        [Description("禁言时长")] uint? minutes)
    {
        uint time = minutes ?? 10U;

        try
        {
            return await bot.GroupMuteMember(group.GroupUin, at.Uin, time * 60)
                ? (new($"禁言 [{at.Uin}] {minutes}分钟"))
                : (new(await StringResources.UnknownErrorMessage.RandomGetAsync()));
        }
        catch (OperationFailedException e)
        {
            Console.WriteLine(e.Message);
            return new(await StringResources.OperationFailedMessage.RandomGetAsync());
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
                : (new(await StringResources.UnknownErrorMessage.RandomGetAsync()));
        }
        catch (OperationFailedException e)
        {
            Console.WriteLine($"{e.Message} ({e.HResult})");
            return new(await StringResources.OperationFailedMessage.RandomGetAsync());
        }
    }
}
