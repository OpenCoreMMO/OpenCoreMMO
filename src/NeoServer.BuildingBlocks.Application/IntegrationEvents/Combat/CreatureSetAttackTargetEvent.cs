using NeoServer.BuildingBlocks.Infrastructure;
using NeoServer.Game.Common.Contracts.Creatures;

namespace NeoServer.BuildingBlocks.Application.IntegrationEvents.Combat;

public record CreatureSetAttackTargetEvent(ICreature Aggressor, ICreature Target) : IIntegrationEvent;