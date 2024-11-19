using NeoServer.BuildingBlocks.Infrastructure;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items.Types;

namespace NeoServer.BuildingBlocks.Application.IntegrationEvents.World;

public record CreatureWalkedThoughMagicFieldEvent(ICreature Creature, IMagicField MagicField): IIntegrationEvent;