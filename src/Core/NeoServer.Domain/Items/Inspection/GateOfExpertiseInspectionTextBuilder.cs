using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Item;

namespace NeoServer.Domain.Items.Inspection;

public static class GateOfExpertiseInspectionTextBuilder
{
    public const int LevelDoorActionIdBase = 1000;

    private const string GateOfExpertiseName = "gate of expertise";
    private const string WorthyLine = "Only the worthy may pass.";

    public static bool TryGetMinLevel(IItem item, out int minLevel)
    {
        minLevel = 0;

        if (item.ActionId == 0)
        {
            return false;
        }

        if (!TryGetBaseActionId(item, out var baseActionId))
        {
            return false;
        }

        if (item.ActionId < baseActionId)
        {
            return false;
        }

        minLevel = item.ActionId - baseActionId;
        return true;
    }

    public static string BuildLevelSuffix(IItem item)
    {
        if (!TryGetMinLevel(item, out var minLevel))
        {
            return string.Empty;
        }

        if (minLevel == 0)
        {
            return " for any level";
        }

        return $" for level {minLevel}";
    }

    public static string BuildWorthyLine(IItem item, bool isClose)
    {
        if (!isClose)
        {
            return string.Empty;
        }

        if (!string.IsNullOrWhiteSpace(item.Metadata.Description))
        {
            return string.Empty;
        }

        if (!TryGetMinLevel(item, out var minLevel))
        {
            return string.Empty;
        }

        if (minLevel == 0)
        {
            return string.Empty;
        }

        return WorthyLine;
    }

    private static bool TryGetBaseActionId(IItem item, out int baseActionId)
    {
        baseActionId = 0;

        if (item.Metadata.Attributes.TryGetAttribute(ItemTypeAttribute.LevelDoor, out int levelDoorBase))
        {
            baseActionId = levelDoorBase;
            return true;
        }

        if (!IsGateOfExpertiseDoor(item))
        {
            return false;
        }

        baseActionId = LevelDoorActionIdBase;
        return true;
    }

    private static bool IsGateOfExpertiseDoor(IItem item)
    {
        return item.Metadata.Attributes.GetAttribute(ItemTypeAttribute.Type) == "door"
               && string.Equals(item.Name, GateOfExpertiseName, StringComparison.OrdinalIgnoreCase);
    }
}
