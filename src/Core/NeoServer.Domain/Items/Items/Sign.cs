using System.Globalization;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Items.Bases;

namespace NeoServer.Domain.Items.Items;

public class Sign : BaseItem
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