using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Items.Bases;

namespace NeoServer.Domain.Items.Items;

public class Sign : BaseItem
{
    public Sign(IItemType metadata, Location location) : base(metadata, location)
    {
    }

    public string Text => Attributes.GetAttribute(ItemAttribute.Text);

    public override string GetLookText(bool isClose = false,
        bool showInternalDetails = false)
    {
        var lookText = base.GetLookText(isClose, showInternalDetails);

        return string.IsNullOrWhiteSpace(Text) ? lookText : $"{lookText}\nYou read: {Text.AddEndOfSentencePeriod()}";
    }

    public static bool IsApplicable(IItemType type, IDictionary<ItemAttribute, IConvertible> attributes)
    {

        return
            type.Group == ItemGroup.Sign ||
            (attributes.ContainsKey(ItemAttribute.Text) && !type.Flags.Contains(ItemFlag.Usable)) ||
            (type.Attributes.GetAttribute(ItemTypeAttribute.Type)
                ?.Equals("sign", StringComparison.InvariantCultureIgnoreCase) ?? false);
    }
}