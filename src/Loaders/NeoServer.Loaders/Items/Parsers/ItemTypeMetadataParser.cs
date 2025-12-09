using System.Collections.Generic;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Items;
using NeoServer.Loaders.Extensions;
using NeoServer.Loaders.OTB.Parsers;

namespace NeoServer.Loaders.Items.Parsers;

public class ItemTypeMetadataParser(Dictionary<ushort, IItemType> itemTypes)
{
    /// <summary>
    ///     Parses ItemNode object to IItemType
    /// </summary>
    /// <returns></returns>
    public void AddMetadata(ItemTypeMetadata metadata, ushort itemTypeId)
    {
        var id = itemTypeId;

        if (id is > 30000 and < 30100) id -= 30000;

        if (!itemTypes.TryGetValue(id, out var itemType)) return;

        itemType.SetName(metadata.Name);
        itemType.SetArticle(metadata.Article);
        itemType.SetPlural(metadata.Plural);

        if (metadata.Flags != null)
        {
            foreach (var flagName in metadata.Flags)
            {
                if (!ItemAttributeTranslation.TranslateFlagName(flagName, out var flag))
                {
                    continue;
                }

                itemType.Flags.Add(flag);
            }
        }

        if (metadata.Attributes == null)
        {
            itemType.SetGroupIfNone();
            return;
        }

        SetAttributes(metadata.Attributes, itemType.Attributes);

        if (metadata.OnUseEvent == null)
        {
            itemType.SetGroupIfNone();
            return;
        }

        foreach (var attribute in metadata.OnUseEvent)
        {
            var itemAttribute = ItemAttributeTranslation.Translate(attribute.Key, out _);
            itemType.SetOnUse();

            var value = JsonTextExtensions.ParseFromJson(attribute.Value);

            if (itemAttribute == ItemTypeAttribute.None)
                itemType.OnUse.SetCustomAttribute(attribute.Key, value);
            else
                itemType.OnUse.SetAttribute(itemAttribute, value);
        }

        itemType.SetGroupIfNone();
    }

    private static void SetAttributes(ItemTypeMetadata.Attribute[] metaAttributes,
        ItemTypeAttributeList attributes)
    {
        foreach (var attribute in metaAttributes)
        {
            var itemAttribute = ItemAttributeTranslation.Translate(attribute.Key, out _);

            var originalValue = JsonTextExtensions.ParseFromJson(attribute.Value);

            var value = itemAttribute == ItemTypeAttribute.Weight
                ? int.Parse(originalValue) / 100f
                : originalValue;

            if (attribute.Attributes == null || attribute.Attributes.Length == 0)
            {
                if (JsonTextExtensions.IsJsonArray(attribute.Value))
                {
                    if (itemAttribute == ItemTypeAttribute.None)
                        attributes.SetCustomAttribute(attribute.Key, values: value);
                    else
                        attributes.SetAttribute(itemAttribute, values: value);
                }
                else
                {
                    if (itemAttribute == ItemTypeAttribute.None)
                        attributes.SetCustomAttribute(attribute.Key, value);
                    else
                        attributes.SetAttribute(itemAttribute, value);
                }
            }
            else
            {
                var innerAttributes = new ItemTypeAttributeList();
                SetAttributes(attribute.Attributes, innerAttributes);

                if (itemAttribute == ItemTypeAttribute.None)
                    attributes.SetCustomAttribute(attribute.Key, value, innerAttributes);
                else
                    attributes.SetAttribute(itemAttribute, value, innerAttributes);
            }
        }
    }
}