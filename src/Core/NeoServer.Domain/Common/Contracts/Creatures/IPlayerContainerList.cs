using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types;

namespace NeoServer.Domain.Common.Contracts.Creatures;

public delegate void RemoveItemFromOpenedContainer(IPlayer player, byte containerId, byte slotIndex, IItem item);

public delegate void AddItemOnOpenedContainer(IPlayer player, byte containerId, IItem item);

public delegate void UpdateItemOnOpenedContainer(IPlayer player, byte containerId, byte slotIndex, IItem item,
    sbyte amount);

public delegate void MoveOpenedContainer(byte containerId, IContainer container);

public interface IPlayerContainerList
{
    IContainer this[byte id] { get; }

    RemoveItemFromOpenedContainer RemoveItemAction { get; set; }
    AddItemOnOpenedContainer AddItemAction { get; set; }
    UpdateItemOnOpenedContainer UpdateItemAction { get; set; }
    bool HasAnyDepotOpened { get; }

    event ClosedContainer OnClosedContainer;
    event OpenedContainer OnOpenedContainer;

    void CloseAll();
    void CloseContainer(byte containerId);
    void CloseDistantContainers();
    void GoBackContainer(byte containerId);

    void MoveItemBetweenContainers(Location.Structs.Location fromLocation, Location.Structs.Location toLocation,
        byte count = 1);

    void OpenContainerAt(IContainer containerToOpen, byte containerLevel);
    bool IsOpened(byte containerId);
    event ClosedDepot OnClosedDepot;
}