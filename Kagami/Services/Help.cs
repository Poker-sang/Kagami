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
    private static string Spacing(int n)
    {
        var r = "";
        for (var i = 0; i < n; i++)
            r += "  ";
        return r;
    }

    private const string cacheHelpImagePath = ".help.png";

    // 改用 "https://hcti.io/v1/image" 以获得最佳体验
    private const string uri = "https://htmlcsstoimage.com/demo_run";

    public static string GenerateHtml()
    {
        var sb = new StringBuilder($@"<div class=""all"">

{Spacing(1)}<h1>PokerKagami 帮助</h1>

{Spacing(1)}<div class=""background""></div>

{Spacing(1)}<div class=""cmd box"">

");

        foreach (var cmdlet in BotResponse.Cmdlets.SelectMany(i => i.Value))
        {
            _ = sb.AppendLine(@$"{Spacing(2)}<div class=""cmd block"">");
            if (cmdlet.Attribute.Permission is not Konata.Core.Common.RoleType.Member)
                _ = sb.AppendLine($@"{Spacing(3)}[<span class=""cmd attribute"">需要{cmdlet.Attribute.Permission switch
                    {
                        Konata.Core.Common.RoleType.Admin => "管理员", 
                        Konata.Core.Common.RoleType.Owner => "群主", 
                        _ => "Unknown"
                    }}权限</span>]<br>");

            if (!cmdlet.Attribute.IgnoreCase)
                _ = sb.AppendLine($@"{Spacing(3)}[<span class=""cmd attribute"">此命令区分大小写</span>]<br>");

            _ = sb.AppendLine($@"{Spacing(3)}<span class=""cmd format""><code>{cmdlet.Attribute.Name}</code>{GenerateArgumentList(cmdlet.Parameters)}</span>
{Spacing(3)}<p class=""cmd description"">{cmdlet.Description}</p>
{Spacing(3)}<div class=""cmd arguments"">");

            foreach (var parameter in cmdlet.Parameters)
            {
                if (parameter.Type == typeof(Bot) || parameter.Type == typeof(GroupMessageEvent))
                    continue;

                var typeName = parameter.Type.Name;

                if (parameter.Type.IsGenericType && parameter.Type.GetGenericTypeDefinition() == typeof(Nullable<>))
                    typeName = parameter.Type.GenericTypeArguments[0].Name + "?";

                _ = sb.AppendLine($@"{Spacing(4)}<li><code class=""type"">{typeName}</code><span class=""name"">{parameter.Description}</span></li>");

                if (parameter.Type.IsEnum)
                {
                    _ = sb.AppendLine($@"{Spacing(4)}<ul>");

                    var flat = parameter.Type.CustomAttributes.Any(a => a.AttributeType == typeof(EnumFlatAttribute));
                    foreach (var item in parameter.Type.GetFields())
                    {
                        if (string.Equals(item.Name, "value__"))
                            continue;

                        _ = sb.AppendLine(flat
                            ? @$"{Spacing(5)}<code>{item.Name}</code>"
                            : @$"{Spacing(5)}<li><code>{item.Name}</code>{item.GetCustomAttribute<DescriptionAttribute>()?.Description}</li>");
                    }

                    _ = sb.AppendLine($@"{Spacing(4)}</ul>");
                }
            }

            _ = sb.AppendLine($"{Spacing(3)}</div>")
                .AppendLine($"{Spacing(2)}</div>")
                .AppendLine();
        }

        _ = sb.AppendLine($@"{Spacing(1)}</div>

</div>
");
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
