using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Kagami.Extensions;

internal static class HttpClientExtensions
{
    private static HttpClient? s_client;
    public static HttpClient Client => s_client ??= new(new HttpClientHandler
    {
        AutomaticDecompression = DecompressionMethods.All
    })
    {
        Timeout = new TimeSpan(0, 0, 0, 8000),
        MaxResponseContentBufferSize = ((long)2 << 30) - 1
    };

    public static HttpClient InitializeHeader(this HttpClient client, Dictionary<string, string>? header = null)
    {
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Mozilla", "5.0"));
        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("PokerKagami", "1.0"));
        if (header is not null)
        {
            foreach ((string k, string v) in header)
                client.DefaultRequestHeaders.Add(k, v);
        }
        Debug.WriteLine($"[{nameof(HttpClientExtensions)}]::{nameof(InitializeHeader)}(): Header: [");
        foreach (KeyValuePair<string, IEnumerable<string>> i in client.DefaultRequestHeaders)
        {
            Debug.WriteLine($"  {i.Key}:{string.Join(';', i.Value)},");
        }
        Debug.WriteLine($"]");
        return client;
    }

    public static Task<string> DownloadStringAsync(this string uri, Dictionary<string, string>? header = null)
        => Client.InitializeHeader(header).GetStringAsync(uri);

    public static Task<Stream> DownloadStreamAsync(this string uri, Dictionary<string, string>? header = null)
        => Client.InitializeHeader(header).GetStreamAsync(uri);

    public static Task<byte[]> DownloadBytesAsync(this string uri, Dictionary<string, string>? header = null)
        => Client.InitializeHeader(header).GetByteArrayAsync(uri);

    public static async Task<JsonDocument> DownloadJsonAsync(this string uri, Dictionary<string, string>? header = null)
        => await JsonDocument.ParseAsync(await uri.DownloadStreamAsync(header));
}
