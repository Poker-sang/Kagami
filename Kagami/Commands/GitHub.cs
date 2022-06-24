using Kagami.UsedTypes;
using Konata.Core.Message;
using System.ComponentModel;
using static Kagami.Services.GitHub;

namespace Kagami.Commands;
public static class GitHub
{
    [Cmdlet(nameof(GitHub)), Description("获取仓库概要图片")]
    public static async Task<MessageBuilder> GetGitHub(
        [Description("组织名")] string owner,
        [Description("仓库名")] string repo)
        => await GetRepoInfoFrom(owner, repo);
}
