using Konata.Core.Message;

namespace Kagami.Services;

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
