using Kagami.ArgTypes;
using System.Text.Json;

namespace Kagami.Services;
internal static class GoogleTranslation
{
    public static async ValueTask<string> TranslateText(this string input, Languages sl, Languages tl)
    {
        var sourceLang = sl is Languages.Cn ? "zh-CN" : sl.ToString().ToLowerInvariant();
        var toLang = tl is Languages.Cn ? "zh-CN" : tl.ToString().ToLowerInvariant();
        // Set the language from/to in the url (or pass it into this function)
        var result = await $"https://translate.googleapis.com/translate_a/single?client=gtx&sl={sourceLang}&tl={toLang}&dt=t&q={Uri.EscapeDataString(input)}"
            .DownloadStringAsync();

        // Get all json data

        if (JsonSerializer.Deserialize<JsonElement[]>(result) is not { } jsonData)
            return "解析失败了";

        // Extract just the first array element (This is the only data we are interested in)
        var translationItems = jsonData[0];

        // Translation Data
        // Loop through the collection extracting the translated objects
        var translation = translationItems.EnumerateArray().Aggregate("", (a, b) => a += $" {b.EnumerateArray().First()}");

        // Remove first blank character
        if (translation.Length > 1)
            translation = translation[1..];
        else
            return "解析失败了";

        // Return translation
        return translation;
    }
}
