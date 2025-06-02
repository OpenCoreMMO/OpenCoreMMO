using NeoServer.Game.Common;
using NeoServer.Game.Common.Combat.Structs;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;

namespace NeoServer.Game.Combat.Services.Attacks.Events;

public record CreatureInjuredEvent(IThing Enemy, ICreature Victim, CombatDamageList DamageList) : IEvent;