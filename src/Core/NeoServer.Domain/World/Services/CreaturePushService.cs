using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Services;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Common.Services;
using NeoServer.Domain.Common.Texts;
using NeoServer.Domain.Creatures.Services;

namespace NeoServer.Domain.World.Services;

public class CreaturePushService(
    IMap map,
    ICreatureMovementService creatureMovementService,
    IWalkToMechanism walkToMechanism) : ICreaturePushService
{
    public void PushCreature(IPlayer player, ICreature target, Location destination)
    {
        if (!target.IsCloseTo(player))
        {
            walkToMechanism.WalkTo(player, () => PushCreature(player, target, destination), target.Location);
            return;
        }

        // Check if the destination to the target is within 1 tile
        var distance = target.Location.GetMaxSqmDistance(destination);
        if (distance > 1)
        {
            OperationFailService.Send(player.CreatureId, TextConstants.DESTINATION_IS_OUT_OF_REACH);
            return;
        }
        
        // Check if the destination tile has another creature
        if (map[destination] is IDynamicTile { HasAnyCreature: true })
        {
            OperationFailService.Send(player.CreatureId, TextConstants.NOT_ENOUGH_ROOM);
            return;
        }

        // Check if the destination tile is a teleport, stairs, holes, or any floor changer
        if (map[destination] is IDynamicTile destinationTile && (destinationTile.HasFlag(TileFlags.Teleport) || 
                                                                 destinationTile.HasFlag(TileFlags.FloorChange)))
        {
            OperationFailService.Send(player.CreatureId, TextConstants.NOT_POSSIBLE);
            return;
        }

        // Check if the target can be pushed
        if (target is IMonster targetMonster)
        {
            var pushingToProtectionZone = map[destination] is IDynamicTile { ProtectionZone: true };
            
            if (!targetMonster.IsPushable || pushingToProtectionZone)
            {
                OperationFailService.Send(player.CreatureId, TextConstants.NOT_POSSIBLE);
                return;
            }
        }

        if (target is INpc)
        {
            var pushingToProtectionZone = map[destination] is IDynamicTile { ProtectionZone: true };
            if (pushingToProtectionZone)
            {
                OperationFailService.Send(player.CreatureId, TextConstants.NOT_POSSIBLE);
                return;
            }
        }

        // For players, check protection zone rules
        if (target is IPlayer targetPlayer)
        {
            var pushingOutsideProtectionZone = map[destination] is IDynamicTile { ProtectionZone: false } &&
                                               (targetPlayer.Tile?.ProtectionZone ?? false);

            if (pushingOutsideProtectionZone)
            {
                OperationFailService.Send(player.CreatureId, TextConstants.NOT_POSSIBLE);
                return;
            }
        }

        // Perform the push
        if (target is (IMonster or INpc) and IWalkableCreature walkableTarget)
            // Use WalkTo for monsters (sends messages)
        {
            walkableTarget.WalkTo(destination);
            return;
        }
        
        if (target is IPlayer playerTarget)
        {
            // Use direct move for players (no messages sent to them)
            creatureMovementService.MoveCreature(playerTarget, destination);
        }
    }
}