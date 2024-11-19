using NeoServer.BuildingBlocks.Infrastructure;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;

namespace NeoServer.BuildingBlocks.Application.IntegrationEvents.Combat;

public record CreatureDiedEvent(ICreature Creature, IThing By) : IIntegrationEvent;