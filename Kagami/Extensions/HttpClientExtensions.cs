using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Kagami.Extensions;

internal static class HttpClientExtensions
{
    public static void Initalize() { }

    private static readonly Queue<Task> task = new();

    public static HttpClient Client { get; } = new(new HttpClientHandler
    {
        AutomaticDecompression = DecompressionMethods.All
    })
    {
        Timeout = new TimeSpan(0, 0, 0, 8000),
        MaxResponseContentBufferSize = ((long)2 << 30) - 1
    };
    private static bool shouldRefreshHeader = true;
    public static HttpClient InitializeHeader(this HttpClient client, Dictionary<string, string>? header = null)
    {
        if (!shouldRefreshHeader && header is null)
            return client;

        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Mozilla", "5.0"));
        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("PokerKagami", "1.0"));
        shouldRefreshHeader = false;
        if (header is not null)
        {
            shouldRefreshHeader = true;
            foreach (var (k, v) in header)
                client.DefaultRequestHeaders.Add(k, v);
        }

        Debug.WriteLine($"[{nameof(HttpClientExtensions)}]::{nameof(InitializeHeader)}(): Header: [");
        foreach (var i in client.DefaultRequestHeaders)
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
