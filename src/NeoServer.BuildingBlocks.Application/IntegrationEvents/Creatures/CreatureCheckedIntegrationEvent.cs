using NeoServer.BuildingBlocks.Infrastructure;
using NeoServer.Game.Common.Contracts.Creatures;

namespace NeoServer.BuildingBlocks.Application.IntegrationEvents.Creatures;

public record CreatureCheckedIntegrationEvent(ICreature Creature) : IIntegrationEvent;