using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace NeoServer.Domain.Common.Helpers;

[ExcludeFromCodeCoverage]
public static class StringBuilderExtension
{
    public static StringBuilder AppendNewLine(this StringBuilder stringBuilder)
    {
        return stringBuilder.Append('\n');
    }

    public static StringBuilder AppendNewLine(this StringBuilder stringBuilder, string value)
    {
        return stringBuilder.Append($"{value}\n");
    }
}