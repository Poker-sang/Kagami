using Kagami.Attributes;
using Kagami.Core;
using Kagami.Records;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Konata.Core.Events.Model;
using Konata.Core;
using System.Text.Json.Serialization;

namespace Kagami.Services;
public static class Help
{
    private const string spacing1 = "  ";
    private const string spacing2 = "    ";
    private const string spacing3 = "      ";
    private const string spacing4 = "        ";
    private const string spacing5 = "          ";

    private const string cacheHelpImagePath = ".help.png";
    private const string htmlBlockArgumentsEnumItem = spacing5 + @"<li><code>{0}</code>{1}</li>";
    private const string htmlBlockArgumentsEnumItemFlat = spacing5 + @"<code>{0}</code>";
    private const string htmlBlockAttribute = spacing3 + @"[<span class=""cmd attribute"">{0}</span>]<br>";
    private const string htmlBlockDescription = spacing3 + @"<p class=""cmd description"">{0}</p>";
    private const string htmlBlockName = spacing3 + @"<span class=""cmd format""><code>{0}</code>{1}</span>";
    private const string htmlBlockArgumentsEnumStart = spacing4 + @"<ul>";
    private const string htmlBlockArgumentsEnumEnd = spacing4 + @"</ul>";
    private const string htmlBlockArgumentsName = spacing4 + @"<li><code class=""type"">{0}</code><span class=""name"">{1}</span></li>";
    private const string htmlBlockArgumentsStart = spacing3 + @"<div class=""cmd arguments"">";
    private const string htmlBlockArgumentsEnd = spacing3 + htmlFooter;
    private const string htmlBlockStart = spacing2 + @"<div class=""cmd block"">";
    private const string htmlBlockEnd = spacing2 + htmlFooter;
    private const string htmlBackGround = spacing1 + @"<div class=""background""></div>";
    private const string htmlBoxHeader = spacing1 + @"<div class=""cmd box"">";
    private const string htmlBoxFooter = spacing1 + @"</div>";
    private const string htmlHeader1 = spacing1 + @"<h1>PokerKagami 帮助</h1>";
    private const string htmlHeader = @"<div class=""all"">";
    private const string htmlFooter = @"</div>";

    // 改用 "https://hcti.io/v1/image" 以获得最佳体验
    private const string uri = "https://htmlcsstoimage.com/demo_run";

    public static string GenerateHtml()
    {
        var sb = new StringBuilder()
            .AppendLine(htmlHeader)
            .AppendLine()
            .AppendLine(htmlHeader1)
            .AppendLine()
            .AppendLine(htmlBackGround)
            .AppendLine()
            .AppendLine(htmlBoxHeader)
            .AppendLine();

        foreach (var cmdlet in BotResponse.Cmdlets.SelectMany(i => i.Value))
        {
            _ = sb.AppendLine(htmlBlockStart);
            if (cmdlet.Attribute.Permission is not Konata.Core.Common.RoleType.Member)
                _ = sb.AppendLine(string.Format(htmlBlockAttribute, $"需要{cmdlet.Attribute.Permission switch
                {
                    Konata.Core.Common.RoleType.Admin => "管理员",
                    Konata.Core.Common.RoleType.Owner => "群主",
                    _ => "Unknown"
                }}权限"));

            if (!cmdlet.Attribute.IgnoreCase)
                _ = sb.AppendLine(string.Format(htmlBlockAttribute, "此命令区分大小写"));

            _ = sb.AppendLine(string.Format(htmlBlockName, cmdlet.Attribute.Name, GenerateArgumentList(cmdlet.Parameters)));
            _ = sb.AppendLine(string.Format(htmlBlockDescription, cmdlet.Description));
            _ = sb.AppendLine(htmlBlockArgumentsStart);

            foreach (var parameter in cmdlet.Parameters)
            {
                if (parameter.Type == typeof(Bot) || parameter.Type == typeof(GroupMessageEvent))
                    continue;

                var typeName = parameter.Type.Name;

                if (parameter.Type.IsGenericType && parameter.Type.GetGenericTypeDefinition() == typeof(Nullable<>))
                    typeName = parameter.Type.GenericTypeArguments[0].Name + "?";

                _ = sb.AppendLine(string.Format(htmlBlockArgumentsName, typeName, parameter.Description));

                if (parameter.Type.IsEnum)
                {
                    _ = sb.AppendLine(htmlBlockArgumentsEnumStart);

                    var flat = parameter.Type.CustomAttributes.Any(a => a.AttributeType == typeof(EnumFlatAttribute));
                    foreach (var item in parameter.Type.GetFields())
                    {
                        if (string.Equals(item.Name, "value__"))
                            continue;

                        _ = sb.AppendLine(string.Format(flat
                            ? htmlBlockArgumentsEnumItemFlat
                            : htmlBlockArgumentsEnumItem,
                            item.Name,
                            item.GetCustomAttribute<DescriptionAttribute>()?.Description));
                    }

                    _ = sb.AppendLine(htmlBlockArgumentsEnumEnd);
                }
            }

            _ = sb.AppendLine(htmlBlockArgumentsEnd)
                .AppendLine(htmlBlockEnd)
                .AppendLine();
        }

        _ = sb.AppendLine(htmlBoxFooter)
            .AppendLine()
            .AppendLine(htmlFooter);
        Debug.WriteLine(sb);
        return sb.ToString();
    }
    private record RequestArgs(
        [property: JsonPropertyName("html")] string Html,
        [property: JsonPropertyName(name: "css")] string Css,
        [property: JsonPropertyName(name: "viewport_width")] uint ViewportWidth = 900,
        [property: JsonPropertyName(name: "viewport_height")] uint ViewportHeight = 1500
    );
    public static async Task<byte[]?> GenerateImageAsync(bool force = false)
    {
        if (File.Exists(cacheHelpImagePath) && !force)
            return await File.ReadAllBytesAsync(cacheHelpImagePath);

        var bytes = await GenerateImageWithoutCacheAsync();

        await using var fs = File.Create(cacheHelpImagePath);
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
        var response = await client.PostAsync(uri, JsonContent.Create(new RequestArgs(html, css)));
        var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        var imgUri = json.RootElement.GetProperty("url").GetString();
        return imgUri is null ? throw new InvalidOperationException("请求失败") : await imgUri.DownloadBytesAsync();
    }

    private static string GenerateArgumentList(KagamiParameter[] parameters)
    {
        var sb = new StringBuilder();
        foreach (var parameter in parameters)
        {
            if (parameter.Type == typeof(Bot) || parameter.Type == typeof(GroupMessageEvent))
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
