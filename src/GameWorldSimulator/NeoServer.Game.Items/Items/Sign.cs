using System;
using System.Collections.Generic;
using System.Globalization;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Helpers;
using NeoServer.Game.Common.Item;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Game.Items.Bases;

namespace NeoServer.Game.Items.Items;

public class Sign : BaseItem, ISign
{
    public Sign(IItemType metadata, Location location, IDictionary<ItemAttribute, IConvertible> attributes) : base(
        metadata, location)
    {
        attributes.TryGetValue(ItemAttribute.Text, out var text);
        Text = text?.ToString(CultureInfo.InvariantCulture);
    }

    public string Text { get; }

    public override string GetLookText(bool isClose = false,
        bool showInternalDetails = false)
    {
        var lookText = base.GetLookText(isClose, showInternalDetails);

        return string.IsNullOrWhiteSpace(Text) ? lookText : $"{lookText}\nYou read: {Text.AddEndOfSentencePeriod()}";
    }

    public static bool IsApplicable(IItemType type)
    {
        return type.Group is ItemGroup.Paper;
    }
}