using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Items;
using NeoServer.Domain.Items.Items;

namespace NeoServer.Domain.Houses;

public class HouseTransferItem : Paper
{
    public const ushort DocumentItemId = 1968;

    private readonly Action<IPlayer> _onComplete;

    public HouseTransferItem(IItemType metadata, House house, Action<IPlayer> onComplete)
        : base(metadata, Location.Container(0, 0))
    {
        House = house;
        _onComplete = onComplete;
    }

    public House House { get; }

    public static IItemType CreateMetadata(IItemType source = null)
    {
        var type = new ItemType();
        type.SetId(DocumentItemId);
        type.SetClientId(source?.ClientId ?? DocumentItemId);
        type.SetName(string.IsNullOrWhiteSpace(source?.Name) ? "document" : source.Name);
        type.SetArticle(string.IsNullOrWhiteSpace(source?.Article) ? "a" : source.Article);

        var weight = source is { Weight: > 0 } ? source.Weight : 1.5f;
        type.Attributes.SetAttribute(ItemTypeAttribute.Weight, weight);
        type.SetFlag(ItemFlag.Pickupable);
        type.SetFlag(ItemFlag.Movable);
        return type;
    }

    public static HouseTransferItem Create(IItemType metadata, House house, IPlayer seller, Action<IPlayer> onComplete)
    {
        var item = new HouseTransferItem(metadata, house, onComplete);
        item.SetOwner(seller);
        item.Attributes.SetAttribute(ItemAttribute.Description,
            $"It is a house transfer document for '{house.Name}'.");
        return item;
    }

    public override string GetLookText(bool isClose = false, bool showInternalDetails = false)
    {
        var lookText = base.GetLookText(isClose, showInternalDetails);
        var description = Attributes.GetAttribute(ItemAttribute.Description);
        if (string.IsNullOrWhiteSpace(description))
        {
            return lookText;
        }

        return $"{lookText}\n{description}";
    }

    public void Complete(IPlayer newOwner)
    {
        if (newOwner is null)
        {
            return;
        }

        if (!House.ExecuteTransfer(this))
        {
            return;
        }

        _onComplete?.Invoke(newOwner);
    }

    public void Cancel()
    {
        House.ResetTransfer();
    }
}
