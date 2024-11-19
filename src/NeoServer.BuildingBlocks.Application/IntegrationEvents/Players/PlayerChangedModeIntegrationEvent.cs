using NeoServer.BuildingBlocks.Infrastructure;
using NeoServer.Game.Common.Contracts.Creatures;

namespace NeoServer.BuildingBlocks.Application.IntegrationEvents.Players;

public record PlayerChangedChaseModeIntegrationEvent(IPlayer Player) : IIntegrationEvent;