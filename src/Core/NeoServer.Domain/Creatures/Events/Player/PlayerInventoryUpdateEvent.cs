using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Creatures.Player.Inventory;

namespace NeoServer.Domain.Creatures.Events.Player;

public record PlayerInventoryUpdateEvent(IPlayer Player, IItem Item, Slot Slot, bool Equip) : IEvent;