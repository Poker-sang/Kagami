using Konata.Core.Message;

namespace Kagami.Services;

/// <summary>
/// 和Github相关的业务逻辑代码
/// </summary>
public static class GitHub
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
            string? html = await $"https://github.com/{owner}/{repo}.git".DownloadStringAsync();
            // Get meta data
            Dictionary<string, string>? metaData = html.GetMetaData("property");
            string? imageMeta = metaData["og:image"];

            // Build message
            byte[]? image = await imageMeta.DownloadBytesAsync();
            return new MessageBuilder().Image(image);
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"Not a repository link. \n{e.Message}");
            return new("不是一个仓库链接");
        }
    }
}
