using Kagami.Attributes;
using Konata.Core;
using Konata.Core.Events.Model;
using Konata.Core.Interfaces.Api;
using Konata.Core.Message;
using System.ComponentModel;
using static Kagami.Services.GitHub;

namespace Kagami.Commands;
public static class GitHub
{
    [KagamiCmdlet(nameof(GitHub)), Description("获取仓库概要图片")]
    public static async Task<MessageBuilder> GetGitHub(Bot bot, GroupMessageEvent group,
        [Description("组织名")] string owner,
        [Description("仓库名")] string repo)
    {
        _ = await bot.SendGroupMessage(group.GroupUin, new MessageBuilder("获取仓库中..."));
        return await GetRepoInfoFrom(owner, repo);
    }
}
