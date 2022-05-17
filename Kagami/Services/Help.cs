using System.ComponentModel;
using System.Net.Http.Json;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kagami.Services;
public static class Help
{
    private const string CACHE_HELP_IMAGE_PATH = ".help.png";
    private const string HTML_BLOCK_ARGUMENTS_ENUM_END = @"</ul>";
    private const string HTML_BLOCK_ARGUMENTS_ENUM_ITEM = @"<li><code>{0}</code>{1}</li>";
    private const string HTML_BLOCK_ARGUMENTS_ENUM_START = @"<ul>";
    private const string HTML_BLOCK_ARGUMENTS_NAME = @"<li><code class=""type"">{0}</code><code class=""name"">{1}</code>{2}</li>";
    private const string HTML_BLOCK_ARGUMENTS_START = @"<div class=""cmd arguments"">";
    private const string HTML_BLOCK_ATTRIBUTE = @"[<span class=""cmd attribute"">{0}</span>]<br>";
    private const string HTML_BLOCK_DESCRIPTION = @"<p class=""cmd description"">{0}</p>";
    private const string HTML_BLOCK_END = @"</div>";
    private const string HTML_BLOCK_NAME = @"<span class=""cmd format""><code>{0}</code>{1}</span>";
    private const string HTML_BLOCK_START = @"<div class=""cmd block"">";
    private const string HTML_FOOTER = @"</main>";
    private const string HTML_HEADER = @"<main class=""cmd box"">";

    // 改用 "https://hcti.io/v1/image" 以获得最佳体验
    private const string URI = "https://htmlcsstoimage.com/demo_run";

    public static string GenerateHtml()
    {
        StringBuilder sb = new(HTML_HEADER);
        foreach (var cmdlet in Entry.Cmdlets.SelectMany(i => i.Value))
        {
            sb.AppendLine(HTML_BLOCK_START);
            if (cmdlet.Permission is not Konata.Core.Common.RoleType.Member)
            {
                sb.AppendLine(string.Format(HTML_BLOCK_ATTRIBUTE, "需要" + cmdlet.Permission switch
                {
                    Konata.Core.Common.RoleType.Admin => "管理员",
                    Konata.Core.Common.RoleType.Owner => "群主",
                    _ => "Unknown"
                } + "特权"));
            }
            if (!cmdlet.IgnoreCase)
            {
                sb.AppendLine(string.Format(HTML_BLOCK_ATTRIBUTE, "此命令区分大小写"));
            }

            sb.AppendLine(string.Format(HTML_BLOCK_NAME, cmdlet.Name, GenerateArgumentList(cmdlet.Parameters)));
            sb.AppendLine(string.Format(HTML_BLOCK_DESCRIPTION, cmdlet.Description));
            sb.AppendLine(HTML_BLOCK_ARGUMENTS_START);
            foreach (var parameter in cmdlet.Parameters)
            {
                if (parameter.Type == typeof(Konata.Core.Bot) || parameter.Type == typeof(Konata.Core.Events.Model.GroupMessageEvent))
                    continue;

                StringBuilder desc = new(parameter.Description);
                if (parameter.HasDefault)
                    desc.Append(@"<code class=""default"">")
                        .Append(parameter.Default)
                        .Append("</code>");
                sb.AppendLine(string.Format(HTML_BLOCK_ARGUMENTS_NAME, parameter.Type.Name, parameter.Name, desc.ToString()));
                if (parameter.Type.IsEnum)
                {
                    sb.AppendLine(HTML_BLOCK_ARGUMENTS_ENUM_START);
                    foreach (var item in parameter.Type.GetFields())
                    {
                        if (string.Equals(item.Name, "value__"))
                            continue;
                        sb.AppendLine(string.Format(HTML_BLOCK_ARGUMENTS_ENUM_ITEM, item.Name, item.GetCustomAttribute<DescriptionAttribute>()?.Description));
                    }

                    sb.AppendLine(HTML_BLOCK_ARGUMENTS_ENUM_END);
                }
            }
            sb.AppendLine(HTML_BLOCK_END);
            sb.AppendLine(HTML_BLOCK_END);
            sb.AppendLine();
        }
        sb.AppendLine(HTML_FOOTER);
        return sb.ToString();
    }
    private record class RequestArgs(
        [property: JsonPropertyName("html")] string Html,
        [property: JsonPropertyName(name: "css")] string Css,
        [property: JsonPropertyName(name: "viewport_width")] uint ViewportWidth = 600
    );
    public static async Task<byte[]?> GenerateImageAsync()
    {
        if (File.Exists(CACHE_HELP_IMAGE_PATH))
            return File.ReadAllBytes(CACHE_HELP_IMAGE_PATH);

        var bytes = await GenerateImageWithoutCacheAsync();

        await using var fs = File.Create(CACHE_HELP_IMAGE_PATH);
        await fs.WriteAsync(bytes);
        await fs.FlushAsync();

        return bytes;
    }

    public static async Task<byte[]?> GenerateImageWithoutCacheAsync()
    {
        string html = GenerateHtml();
        string css = await File.ReadAllTextAsync("Assets/style.css");
        var client = HttpClientExtensions.Client.InitializeHeader();

        // 仅在使用 "https://hcti.io/v1/image" 时需要这步操作
        //string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes("user_id:api_key"));
        //client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);
        var response = await client.PostAsync(URI, JsonContent.Create(new RequestArgs(html, css)));
        var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        var imgUri = json.RootElement.GetProperty("url").GetString();
        return imgUri switch
        {
            null => throw new InvalidOperationException("请求失败"),
            _ => await imgUri.DownloadBytesAsync()
        };
    }

    private static string GenerateArgumentList(KagamiCmdletParameter[] parameters)
    {
        StringBuilder sb = new();
        foreach (var parameter in parameters)
        {
            if (parameter.HasDefault)
                sb.Append('[');
            sb.Append(parameter.Name);
            if (parameter.HasDefault)
                sb.Append(" = ")
                    .Append(parameter.Default);
            if (parameter.HasDefault)
                sb.Append(']');

            sb.Append(' ');
        }
        return sb.ToString();
    }
}
