using RainDropWeb.Locale;

namespace RainDropWeb;

public static class Helper
{
    public static byte[] ToByteArray(this string str)
    {
        if ((str.Length & 1) == 1)
            throw new ArgumentException($"{nameof(str)}" + Localization.Localize("BYTE_STR_EVEN_LENGTH"));//"{nameof(str)} must have an even length.");

        var bytes = new byte[str.Length / 2];
        for (var i = 0; i < bytes.Length; ++i)
            bytes[i] = Convert.ToByte(str.Substring(i * 2, 2), 16);

        return bytes;
    }
}