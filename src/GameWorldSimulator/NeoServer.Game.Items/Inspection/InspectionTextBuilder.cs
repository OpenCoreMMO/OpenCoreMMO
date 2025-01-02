using System.Globalization;
using System.Text;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.Items.Types;
using NeoServer.Game.Common.Helpers;

namespace NeoServer.Game.Items.Inspection;

public class InspectionTextBuilder
{
    public static string Build(IThing thing, bool isClose = false, bool showInternalDetails = false)
    {
        if (thing is not IItem item) return string.Empty;

        var inspectionText = new StringBuilder();

        AddItemName(item, showInternalDetails, inspectionText);
        AddEquipmentAttributes(item, inspectionText);
        inspectionText.AppendNewLine(".");
        AddRequirement(item, inspectionText);

        AddWeight(item, isClose, inspectionText);
        AddDescription(item, inspectionText);

        var finalText = inspectionText.ToString().TrimNewLine().AddEndOfSentencePeriod();

        return $"{finalText}";
    }

    public static bool IsApplicable(IThing thing)
    {
        return thing is IItem;
    }

    private static void AddRequirement(IItem item, StringBuilder inspectionText)
    {
        var result = RequirementInspectionTextBuilder.Build(item);
        if (string.IsNullOrWhiteSpace(result)) return;
        inspectionText.AppendNewLine(result);
    }

    private static void AddDescription(IItem item, StringBuilder inspectionText)
    {
        if (!string.IsNullOrWhiteSpace(item.Metadata.Description))
            inspectionText.AppendNewLine(item.Metadata.Description);
    }

    private static void AddWeight(IItem item, bool isClose, StringBuilder inspectionText)
    {
        if (item.IsPickupable && isClose)
            inspectionText.AppendNewLine(
                $"{(item is ICumulative ? "They weigh" : "It weighs")} {item.Weight.ToString("F", CultureInfo.InvariantCulture)} oz.");
    }

    private static void AddItemName(IItem item, bool showInternalDetails, StringBuilder inspectionText)
    {
        if (showInternalDetails)
            inspectionText.AppendNewLine($"Id: [{item.ServerId}] - Pos: {item.Location}");

        inspectionText.Append("You see ");
        inspectionText.Append(item is ICumulative cumulative
            ? $"{cumulative.Amount} {item.Name}{(cumulative.Amount > 1 ? "s" : "")}"
            : $"{item.Metadata.Article} {item.Name}");
    }

    private static void AddEquipmentAttributes(IItem item, StringBuilder inspectionText)
    {
        if (item is IEquipment && !string.IsNullOrWhiteSpace(item.InspectionText))
            inspectionText.Append($" {item.InspectionText}");
    }
}