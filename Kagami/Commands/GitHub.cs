using System.ComponentModel;
using Kagami.Attributes;
using Konata.Core.Message;
using static Kagami.Services.GitHub;

namespace Kagami.Commands;
public static class GitHub
{
    [KagamiCmdlet(nameof(GitHub)), Description("获取仓库概要图片")]
    public static async Task<MessageBuilder> GetGitHub(
        [Description("组织名")] string owner,
        [Description("仓库名")] string repo)
        => await GetRepoInfoFrom(owner, repo);
}
