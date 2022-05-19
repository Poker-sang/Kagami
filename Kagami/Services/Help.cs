using Kagami.Core;
using System.ComponentModel;
using System.Diagnostics;
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
        foreach (var cmdlet in BotResponse.Cmdlets.SelectMany(i => i.Value))
        {
            _ = sb.AppendLine(HTML_BLOCK_START);
            if (cmdlet.Permission is not Konata.Core.Common.RoleType.Member)
            {
                _ = sb.AppendLine(string.Format(HTML_BLOCK_ATTRIBUTE, "需要" + cmdlet.Permission switch
                {
                    Konata.Core.Common.RoleType.Admin => "管理员",
                    Konata.Core.Common.RoleType.Owner => "群主",
                    _ => "Unknown"
                } + "特权"));
            }

            if (!cmdlet.IgnoreCase)
            {
                _ = sb.AppendLine(string.Format(HTML_BLOCK_ATTRIBUTE, "此命令区分大小写"));
            }

            _ = sb.AppendLine(string.Format(HTML_BLOCK_NAME, cmdlet.Name, GenerateArgumentList(cmdlet.Parameters)));
            _ = sb.AppendLine(string.Format(HTML_BLOCK_DESCRIPTION, cmdlet.Description));
            _ = sb.AppendLine(HTML_BLOCK_ARGUMENTS_START);
            foreach (var parameter in cmdlet.Parameters)
            {
                if (parameter.Type == typeof(Konata.Core.Bot) || parameter.Type == typeof(Konata.Core.Events.Model.GroupMessageEvent))
                    continue;

                StringBuilder desc = new(parameter.Description);
                if (parameter.HasDefault)
                    _ = desc.Append(@"<code class=""default"">")
                        .Append(parameter.Default ?? "null")
                        .Append("</code>");

                var typeName = parameter.Type.Name;
                if (parameter.Type.IsGenericType && parameter.Type.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    typeName = parameter.Type.GenericTypeArguments[0].Name;
                    typeName += "?";
                }

                _ = sb.AppendLine(string.Format(HTML_BLOCK_ARGUMENTS_NAME, typeName, parameter.Name, desc.ToString()));
                if (parameter.Type.IsEnum)
                {
                    _ = sb.AppendLine(HTML_BLOCK_ARGUMENTS_ENUM_START);
                    foreach (var item in parameter.Type.GetFields())
                    {
                        if (string.Equals(item.Name, "value__"))
                            continue;
                        _ = sb.AppendLine(string.Format(HTML_BLOCK_ARGUMENTS_ENUM_ITEM, item.Name, item.GetCustomAttribute<DescriptionAttribute>()?.Description));
                    }

                    _ = sb.AppendLine(HTML_BLOCK_ARGUMENTS_ENUM_END);
                }
            }

            _ = sb.AppendLine(HTML_BLOCK_END);
            _ = sb.AppendLine(HTML_BLOCK_END);
            _ = sb.AppendLine();
        }

        _ = sb.AppendLine(HTML_FOOTER);
        Debug.WriteLine(sb);
        return sb.ToString();
    }
    private record RequestArgs(
        [property: JsonPropertyName("html")] string Html,
        [property: JsonPropertyName(name: "css")] string Css,
        [property: JsonPropertyName(name: "viewport_width")] uint ViewportWidth = 600,
        [property: JsonPropertyName(name: "viewport_height")] uint ViewportHeight = 1200
    );
    public static async Task<byte[]?> GenerateImageAsync(bool force = false)
    {
        if (File.Exists(CACHE_HELP_IMAGE_PATH) && !force)
            return File.ReadAllBytes(CACHE_HELP_IMAGE_PATH);

        var bytes = await GenerateImageWithoutCacheAsync();

        await using var fs = File.Create(CACHE_HELP_IMAGE_PATH);
        await fs.WriteAsync(bytes);
        await fs.FlushAsync();

        return bytes;
    }

    public static async Task<byte[]?> GenerateImageWithoutCacheAsync()
    {
        var html = GenerateHtml();
        var css = await File.ReadAllTextAsync("Assets/style.css");
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

    private static string GenerateArgumentList(KagamiParameter[] parameters)
    {
        StringBuilder sb = new();
        foreach (var parameter in parameters)
        {
            if (parameter.Type == typeof(Konata.Core.Bot) || parameter.Type == typeof(Konata.Core.Events.Model.GroupMessageEvent))
                continue;

            if (parameter.HasDefault)
                _ = sb.Append('[');
            _ = sb.Append(parameter.Name);
            if (parameter.HasDefault)
                _ = sb.Append(@" = <code class=""default"">")
                    .Append(parameter.Default ?? "null")
                    .Append("</code>");
            if (parameter.HasDefault)
                _ = sb.Append(']');

            _ = sb.Append(' ');
        }

        return sb.ToString();
    }
}
