using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Items.Items.Containers.Container;

namespace NeoServer.Domain.Locker;

public class Locker : Container
{
    public Locker(IItemType type, Location location, IEnumerable<IItem> children) : base(type, location, children)
    {
    }

    private uint OpenedBy { get; set; }
    public override byte Capacity => 2;
    public bool IsAlreadyOpened { get; private set; }

    public override void ClosedBy(IPlayer player)
    {
        if (RootParent is not Locker || player.HasDepotOpened) return;
        SetAsClosed();
        base.ClosedBy(player);
    }

    public void SetAsOpened(IPlayer openedBy)
    {
        OpenedBy = openedBy.Id;
        IsAlreadyOpened = true;
    }

    public bool CanBeOpenedBy(IPlayer player)
    {
        if (OpenedBy == player.Id) return true;
        return !IsAlreadyOpened;
    }

    public new static bool IsApplicable(IItemType metadata)
    {
        if (metadata.Group is not ItemGroup.Container) return false;

        var type = metadata.Attributes.GetAttribute(ItemTypeAttribute.Type);
        return type is not null && type.Equals("depot", StringComparison.InvariantCultureIgnoreCase);
    }

    private void SetAsClosed()
    {
        IsAlreadyOpened = false;
    }
}