using Kagami.Attributes;
using Kagami.Core;
using Kagami.Records;
using Konata.Core;
using Konata.Core.Events.Model;
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
    private const string Spacing1 = "  ";
    private const string Spacing2 = "    ";
    private const string Spacing3 = "      ";
    private const string Spacing4 = "        ";
    private const string Spacing5 = "          ";

    private const string CacheHelpImagePath = ".help.png";
    private const string HtmlBlockArgumentsEnumItem = Spacing4 + @"<li><code>{0}</code>{1}</li>";
    private const string HtmlBlockArgumentsEnumItemFlat = Spacing4 + @"<code>{0}</code>";
    private const string HtmlBlockAttribute = Spacing2 + @"[<span class=""cmd attribute"">{0}</span>]<br>";
    private const string HtmlBlockDescription = Spacing2 + @"<p class=""cmd description"">{0}</p>";
    private const string HtmlBlockName = Spacing2 + @"<span class=""cmd format""><code>{0}</code>{1}</span>";
    private const string HtmlBlockArgumentsEnumStart = Spacing3 + @"<ul>";
    private const string HtmlBlockArgumentsEnumEnd = Spacing3 + @"</ul>";
    private const string HtmlBlockArgumentsName = Spacing3 + @"<li><code class=""type"">{0}</code><span class=""name"">{1}</span></li>";
    private const string HtmlBlockArgumentsStart = Spacing2 + @"<div class=""cmd arguments"">";
    private const string HtmlBlockArgumentsEnd = Spacing2 + HtmlFooter;
    private const string HtmlBlockStart = Spacing1 + @"<div class=""cmd block"">";
    private const string HtmlBlockEnd = Spacing1 + HtmlFooter;
    private const string HtmlHeader = @"<div class=""cmd box"">";
    private const string HtmlFooter = @"</div>";

    // 改用 "https://hcti.io/v1/image" 以获得最佳体验
    private const string Uri = "https://htmlcsstoimage.com/demo_run";

    public static string GenerateHtml()
    {
        var sb = new StringBuilder()
            .AppendLine(HtmlHeader)
            .AppendLine();

        foreach (var cmdlet in BotResponse.Cmdlets.SelectMany(i => i.Value))
        {
            _ = sb.AppendLine(HtmlBlockStart);
            if (cmdlet.Attribute.Permission is not Konata.Core.Common.RoleType.Member)
                _ = sb.AppendLine(string.Format(HtmlBlockAttribute, $"需要{cmdlet.Attribute.Permission switch
                {
                    Konata.Core.Common.RoleType.Admin => "管理员",
                    Konata.Core.Common.RoleType.Owner => "群主",
                    _ => "Unknown"
                }}权限"));

            if (!cmdlet.Attribute.IgnoreCase)
                _ = sb.AppendLine(string.Format(HtmlBlockAttribute, "此命令区分大小写"));

            _ = sb.AppendLine(string.Format(HtmlBlockName, cmdlet.Attribute.Name, GenerateArgumentList(cmdlet.Parameters)));
            _ = sb.AppendLine(string.Format(HtmlBlockDescription, cmdlet.Description));
            _ = sb.AppendLine(HtmlBlockArgumentsStart);

            foreach (var parameter in cmdlet.Parameters)
            {
                if (parameter.Type == typeof(Bot) || parameter.Type == typeof(GroupMessageEvent))
                    continue;

                var typeName = parameter.Type.Name;

                if (parameter.Type.IsGenericType && parameter.Type.GetGenericTypeDefinition() == typeof(Nullable<>))
                    typeName = parameter.Type.GenericTypeArguments[0].Name + "?";

                _ = sb.AppendLine(string.Format(HtmlBlockArgumentsName, typeName, parameter.Description));

                if (parameter.Type.IsEnum)
                {
                    _ = sb.AppendLine(HtmlBlockArgumentsEnumStart);

                    var flat = parameter.Type.CustomAttributes.Any(a => a.AttributeType == typeof(EnumFlatAttribute));
                    foreach (var item in parameter.Type.GetFields())
                    {
                        if (string.Equals(item.Name, "value__"))
                            continue;

                        _ = sb.AppendLine(string.Format(flat
                            ? HtmlBlockArgumentsEnumItemFlat
                            : HtmlBlockArgumentsEnumItem,
                            item.Name,
                            item.GetCustomAttribute<DescriptionAttribute>()?.Description));
                    }

                    _ = sb.AppendLine(HtmlBlockArgumentsEnumEnd);
                }
            }

            _ = sb.AppendLine(HtmlBlockArgumentsEnd)
                .AppendLine(HtmlBlockEnd)
                .AppendLine();
        }

        _ = sb.AppendLine(HtmlFooter);
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
        if (File.Exists(CacheHelpImagePath) && !force)
            return await File.ReadAllBytesAsync(CacheHelpImagePath);

        var bytes = await GenerateImageWithoutCacheAsync();

        await using var fs = File.Create(CacheHelpImagePath);
        await fs.WriteAsync(bytes);
        await fs.FlushAsync();

        Console.WriteLine("help更新完成");
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
        var response = await client.PostAsync(Uri, JsonContent.Create(new RequestArgs(html, css)));
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
        var sb = new StringBuilder();
        foreach (var parameter in parameters)
        {
            if (parameter.Type == typeof(Konata.Core.Bot) || parameter.Type == typeof(Konata.Core.Events.Model.GroupMessageEvent))
                continue;

            if (parameter.HasDefault)
                _ = sb.Append('[');
            _ = sb.Append(parameter.Description);
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
