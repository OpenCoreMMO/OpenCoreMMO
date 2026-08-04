using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Creatures.Player.Inventory;

namespace NeoServer.Domain.Items.Events;

public record EquipmentUnequippedEvent(IPlayer Player, IEquipment Equipment, Slot Slot) : IEvent;
