using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Services;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Services;
using NeoServer.Domain.Common.Texts;
using NeoServer.Domain.Creatures.Player;
using NeoServer.Domain.Creatures.Services;

namespace NeoServer.Domain.World.Services;

public class CreaturePushService(
    IMap map,
    ICreatureMovementService creatureMovementService,
    IWalkToMechanism walkToMechanism) : ICreaturePushService
{
    public void PushCreature(IPlayer player, ICreature target, ITile toTile)
    {
        if (target is IPlayer pushedPlayer && pushedPlayer.Group.FlagIsEnabled(PlayerFlag.CannotBePushed))
        {
            OperationFailService.Send(player.CreatureId, TextConstants.NOT_POSSIBLE);
            return;
        }
        
        if (!player.CooldownHasExpired(CooldownType.PushCreature) && !player.Group.Access)
        {
            OperationFailService.Send(player.CreatureId, InvalidOperation.NotPossible);
            return;
        }
        
        if (!player.CanSee(target))
        {
            return;
        }

        if (!target.IsCloseTo(player))
        {
            walkToMechanism.WalkTo(player, () => PushCreature(player, target, toTile), target.Location);
            return;
        }

        // Check if the destination to the target is within 1 tile
        var distance = target.Location.GetMaxSqmDistance(toTile.Location);
        if (distance > 1)
        {
            OperationFailService.Send(player.CreatureId, TextConstants.DESTINATION_IS_OUT_OF_REACH);
            return;
        }

        // Check if the destination tile has another creature
        if (toTile is IDynamicTile { HasAnyCreature: true })
        {
            OperationFailService.Send(player.CreatureId, TextConstants.NOT_ENOUGH_ROOM);
            return;
        }

        // Check if the destination tile is a block path
        if (toTile is IDynamicTile destinationTile && destinationTile.HasFlag(TileFlags.BlockPath))
        {
            OperationFailService.Send(player.CreatureId, TextConstants.NOT_POSSIBLE);
            return;
        }

        // Check if the target can be pushed
        if (target is IMonster targetMonster)
        {
            var pushingToProtectionZone = toTile is IDynamicTile { ProtectionZone: true };

            if (!targetMonster.IsPushable || pushingToProtectionZone)
            {
                OperationFailService.Send(player.CreatureId, TextConstants.NOT_POSSIBLE);
                return;
            }
        }

        if (target is INpc)
        {
            var pushingToProtectionZone = toTile is IDynamicTile { ProtectionZone: true };
            if (pushingToProtectionZone)
            {
                OperationFailService.Send(player.CreatureId, TextConstants.NOT_POSSIBLE);
                return;
            }
        }

        // For players, check protection zone rules
        if (target is IPlayer targetPlayer)
        {
            var pushingOutsideProtectionZone = toTile is IDynamicTile { ProtectionZone: false } &&
                                               (targetPlayer.Tile?.ProtectionZone ?? false);

            if (pushingOutsideProtectionZone)
            {
                OperationFailService.Send(player.CreatureId, TextConstants.NOT_POSSIBLE);
                return;
            }
        }

        player.StartCooldown(CooldownType.PushCreature, 2_000);

        // Perform the push
        if (target is (IMonster or INpc) and IWalkableCreature walkableTarget)
            // Use WalkTo for monsters (sends messages)
        {
            walkableTarget.WalkTo(toTile.Location);
            return;
        }

        if (target is IPlayer playerTarget)
        {
            // Use direct move for players (no messages sent to them)
            creatureMovementService.MoveCreature(playerTarget, toTile.Location);
        }
    }
}