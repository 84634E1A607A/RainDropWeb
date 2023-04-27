using System.Globalization;

namespace RainDropWeb.Locale;

public static class Localization
{
    private static readonly Dictionary<string, string> English = new()
    {
        { "INVALID_WAVE_FUNCTION", "Invalid wave function." }
    };

    private static readonly Dictionary<string, string> Chinese = new()
    {
        { "INVALID_WAVE_FUNCTION", "无效的波形函数。" }
    };

    public static CultureInfo Culture { get; set; } = CultureInfo.CurrentCulture;

    public static string Localize(string key)
    {
        if (Culture.Name == "zh-cn")
        {
            return Chinese.TryGetValue(key, out var value) ? value : key;
        }
        else
        {
            return English.TryGetValue(key, out var value) ? value : key;
        }
    }
}
