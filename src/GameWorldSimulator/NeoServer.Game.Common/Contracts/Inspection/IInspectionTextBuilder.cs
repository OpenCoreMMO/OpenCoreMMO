using System;
using NeoServer.Game.Common.Contracts.Items;

namespace NeoServer.Game.Common.Contracts.Inspection;

public interface IInspectionTextBuilder
{
    bool IsApplicable(IThing thing);

    public static string GetArticle(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "a";

        Span<char> vowels = ['a', 'e', 'i', 'o', 'u'];
        return vowels.Contains(name.ToLower()[0]) ? "an" : "a";
    }
}