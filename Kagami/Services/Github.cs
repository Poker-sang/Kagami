using Kagami.Utils;
using Konata.Core.Message;

namespace Kagami.Services;

/// <summary>
/// 和Github相关的业务逻辑代码
/// </summary>
public static class Github
{
    /// <summary>
    /// 获取仓库信息
    /// </summary>
    /// <param name="owner">所有者</param>
    /// <param name="repo">仓库</param>
    /// <returns></returns>
    public static async Task<MessageBuilder> GetRepoInfoFrom(string owner, string repo)
    {
        try
        {
            // if (text.Content[6..].Trim().Split(' ') is not { Length: 2 } args)
            //     return Text(ArgumentError);
            // _ = await bot.SendGroupMessage(group.GroupUin, Text("获取仓库中..."));
            var html = await $"https://github.com/{owner}/{repo}.git".DownloadStringAsync();
            // Get meta data
            var metaData = html.GetMetaData("property");
            var imageMeta = metaData["og:image"];

            // Build message
            var image = await imageMeta.DownloadBytesAsync();
            return new MessageBuilder().Image(image);
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"Not a repository link. \n{e.Message}");
            return new("不是一个仓库链接");
        }
    }
}