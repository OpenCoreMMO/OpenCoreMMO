using NeoServer.Domain.Common.Item;

namespace NeoServer.Loaders.OTB.Parsers;

public class ItemAttributeTranslation
{
    /// <summary>
    ///     Translate attribute name to enum
    /// </summary>
    public static ItemTypeAttribute Translate(string attrName, out bool success)
    {
        success = true;
        if (!ItemAttributeTranslationMap.Map.TryGetValue(attrName, out var attribute))
        {
            success = false;
            return ItemTypeAttribute.None;
        }

        return attribute;
    }

    public static bool TranslateFlagName(string flagName, out ItemFlag flag)
    {
        flag = ItemFlag.Usable;
        switch (flagName)
        {
            case "useable":
                flag = ItemFlag.Usable;
                break;
            default:
                return false;
        }

        return true;
    }

    public static int TranslateMeeleWeaponTypeName(string typeName, out bool success)
    {
        success = true;
        switch (typeName)
        {
            case "sword":
                return 1;

            case "club":
                return 2;

            case "axe":
                return 3;

            default:
                success = false;
                return 0;
        }
    }

    public static ItemFlag TranslateToItemFlag(string possibleFlag, out bool success)
    {
        success = true;
        switch (possibleFlag)
        {
            case "distance":
                return ItemFlag.Bow;

            case "shield":
                return ItemFlag.Shield;

            case "ammunition":
                return ItemFlag.Ammo;

            case "wand":
                return ItemFlag.Wand;

            default:
                success = false;
                return ItemFlag.Ammo; // Just return the first
        }
    }

    public static int TranslateLiquidType(string type, out bool success)
    {
        success = true;
        switch (type)
        {
            case "water":
                return 1;

            case "wine":
                return 2;

            case "beer":
                return 3;

            case "muddy":
                return 4;

            case "blood":
                return 5;

            case "slime":
                return 6;

            default:
                success = false;
                return 0;
        }
    }

    public static int TranslateCorpseType(string type, out bool success)
    {
        success = true;
        switch (type)
        {
            case "blood":
                return 1;

            case "venom":
                return 2;

            default:
                success = false;
                return 0;
        }
    }

    public static int TranslateSlotType(string type, out bool success)
    {
        success = true;
        switch (type)
        {
            case "two-handed":
                return 0;

            case "head":
                return 1;

            case "necklace":
                return 2;

            case "backpack":
                return 3;

            case "body":
                return 4;

            case "right-hand": // Not sure if right-hand comes first than left-hand
                return 5;

            case "left-hand":
                return 6;

            case "legs":
                return 7;

            case "feet":
                return 8;

            case "ring":
                return 9;

            default:
                success = false;
                return 0;
        }
    }
}