using Kagami.ArgTypes;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Kagami.Services;

public static class Novel
{
    private const string BaseUri = "https://fiction.cyapi.cn/v2/";// "http://if.caiyunai.com/v1/dream/"
    private const string NovelUri = $"{BaseUri}novel/{Token}/";
    private const string Token = "6290927806fbcbd68cb90683";// "tlmf0yyjen3uli2zascm"
    private const string Info = $"{BaseUri}user/{Token}/info";
    private const string Save = NovelUri + "novel_save";
    private const string Ai = NovelUri + "novel_ai";
    // private const string NovelDreamLoop = novelUri + "novel_dream_loop";

    private static readonly Dictionary<NovelDream, string> _mid = new()
    {
        { NovelDream.Bot0, "60094a2a9661080dc490f75a" },
        { NovelDream.Bot1, "601ac4c9bd931db756e22da6"},
        { NovelDream.PureLove, "601f92f60c9aaf5f28a6f908" },
        { NovelDream.Romance, "601f936f0c9aaf5f28a6f90a" },
        { NovelDream.Fantasy, "60211134902769d45689bf75" }
    };

    public static async Task<string> WriteNovel(NovelDream mode, string title, string content)
    {
        var client = HttpClientExtensions.Client.InitializeHeader();

        var infoObject = new StringContent(new JsonObject
        {
            ["ostype"] = "",
            ["lang"] = "zh"
        }.ToJsonString());

        using var infoRes = await client.PostAsync(Info, infoObject);
        if (!infoRes.IsSuccessStatusCode)
            if (!(await client.PostAsync(Info, infoObject)).IsSuccessStatusCode)
                return new("Token出现错误...");

        var saveObject = new StringContent(new JsonObject
        {
            ["title"] = title,
            ["text"] = content,
            ["nodes"] = new JsonArray(),
            ["lang"] = "zh",
            ["ostype"] = ""
        }.ToJsonString());
        using var nidRes = await client.PostAsync(Save, saveObject);
        if (!nidRes.IsSuccessStatusCode)
            return new("获取小说nid错误...");
        var nidData = (await JsonDocument.ParseAsync(await nidRes.Content.ReadAsStreamAsync()))
            .RootElement.GetProperty("data");
        var nid = nidData.GetProperty("nid").GetString()!;

        var requestObject = new StringContent(new JsonObject
        {
            ["branchid"] = nidData.GetProperty("novel").GetProperty("branchid").GetString()!,
            ["content"] = content,
            ["lang"] = "zh",
            ["lastnode"] = nidData.GetProperty("firstnode").GetProperty("nodeid").GetString()!,
            ["mid"] = _mid[mode],
            ["nid"] = nid,
            ["ostype"] = "",
            ["status"] = "http",
            ["storyline"] = false,
            ["title"] = title,
            ["uid"] = Token,
        }.ToJsonString());
        using var xidRes = await client.PostAsync(Ai, requestObject);
        if (!xidRes.IsSuccessStatusCode)
            return new("推送小说xid错误...");
        using var arr = (await JsonDocument.ParseAsync(await xidRes.Content.ReadAsStreamAsync()))
            .RootElement.GetProperty("data").GetProperty("nodes").EnumerateArray();
        _ = arr.MoveNext();
        return content + arr.Current.GetProperty("content");
        // var rst = arr.Aggregate("", (current, a) => current + content + a.GetProperty("content") + "\n\n");
        // return new(rst[..^2]);
    }
}
