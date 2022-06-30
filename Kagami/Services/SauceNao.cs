using System.Text.Json;
using System.Text.Json.Nodes;

namespace Kagami.Services;

public static class SauceNao
{
    private const string Key = "924b8942229c980c731c98e13b89f3a755568e4e";
    private const string Api = "https://saucenao.com/search.php";

    public static async Task<string> Search(string imageUrl)
    {
        var picParams = new StringContent(new JsonObject
        {
            ["url"] = imageUrl,
            ["db"] = 999,
            ["api_key"] = Key,
            ["output_type"] = 2,
            ["numres"] = 3
        }.ToJsonString());

        var client = HttpClientExtensions.Client.InitializeHeader();
        using var res = await client.PostAsync(Api, picParams);
        if (!res.IsSuccessStatusCode)
            return new("获取错误...");
        //var nidData = (await JsonDocument.ParseAsync(await res.Content.ReadAsStreamAsync()))
        //    .RootElement;
        return "s";
    }
}
