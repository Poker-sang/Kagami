using System.Text.RegularExpressions;

namespace Kagami.Services;

public static class Meme
{
    private const string URI_TEMPLATE = "https://cangku.icu/api/v1/post/search?search=沙雕图集锦";
    private const string REGEX = @"<img src=""([\w:/.]+)"" class=""[\w\- ]+"" alt=""[\w.]+"">";

    public static async Task<string[]> GetMemeImageSourcesAsync(string issue!! = "")
    {
        await Task.Yield();

        if (!string.IsNullOrWhiteSpace(issue))
            issue = $" 第{issue}期";

        string uri = URI_TEMPLATE + issue;
        var json = await uri.DownloadJsonAsync();
        string html = json.RootElement.GetProperty("data")[0].GetProperty("content").GetString()!;

        var matches = Regex.Matches(html, REGEX);
        return matches.Select(i => i.Groups[1].Value).ToArray();
    }

}