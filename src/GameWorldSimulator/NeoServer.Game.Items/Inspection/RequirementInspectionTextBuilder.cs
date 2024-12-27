using System.Text;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Helpers;
using NeoServer.Game.Common.Item;

namespace NeoServer.Game.Items.Inspection;

public static class RequirementInspectionTextBuilder
{
    public static string Build(IItem item)
    {
        if (item is not IRequirement itemRequirement) return string.Empty;

        var vocations = itemRequirement.Metadata.Attributes.GetAttributeArray<string>(ItemAttribute.VocationNames);
        var minLevel = itemRequirement.MinLevel;

        if (Guard.IsNullOrEmpty(vocations) && minLevel == 0) return string.Empty;
        var vocationsText = FormatVocations(vocations);

        var verb = itemRequirement switch
        {
            IEquipmentRequirement => "wielded",
            IConsumableRequirement => "consumed",
            IUsableRequirement => "used",
            _ => "wielded"
        };

        return
            $"It can only be {verb} properly by {vocationsText}{(minLevel > 0 ? $" of level {minLevel} or higher" : string.Empty)}.";
    }

    private static string FormatVocations(string[] allVocations)
    {
        if (Guard.IsNullOrEmpty(allVocations)) return "players";
        var text = new StringBuilder();
        for (var i = 0; i < allVocations.Length; i++)
        {
            var vocation = allVocations[i];
         
            text.Append($"{vocation.ToLower()}s");
            var lastItem = i == allVocations.Length - 1;
            var penultimate = i == allVocations.Length - 2;
            if (lastItem) continue;
            if (penultimate)
            {
                text.Append(" and ");
                continue;
            }

            text.Append(", ");
        }

        var finalText = text.ToString();
        return string.IsNullOrWhiteSpace(finalText) ? "players" : text.ToString();
    }
}