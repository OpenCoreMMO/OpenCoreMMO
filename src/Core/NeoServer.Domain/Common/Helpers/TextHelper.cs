namespace NeoServer.Domain.Common.Helpers;

public static class TextHelper
{
    public static string AddEndOfSentencePeriod(this string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        return text[^1] == '.' ? text : $"{text}.";
    }

    public static string TrimNewLine(this string text)
    {
        return text?.TrimEnd('\r', '\n');
    }
}