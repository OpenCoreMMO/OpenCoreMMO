using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Services;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Services;
using NeoServer.Domain.Common.Texts;
using NeoServer.Domain.Creatures.Services;

namespace NeoServer.Domain.World.Services;

public class CreaturePushService(
    ICreatureMovementService creatureMovementService,
    IWalkToMechanism walkToMechanism) : ICreaturePushService
{
    public void PushCreature(IPlayer player, ICreature target, ITile toTile)
    {
        // Handle the case where target is not close - use walk-to mechanism
        if (!target.IsCloseTo(player))
        {
            walkToMechanism.WalkTo(player, () => PushCreature(player, target, toTile), target.Location);
            return;
        }
        
        // Use the new domain validation method
        var canPushResult = player.CanPushCreature(target, toTile);
        
        if (canPushResult.Failed)
        {
            // Send the appropriate error message based on the validation result
            switch (canPushResult.Reason)
            {
                case InvalidOperation.DestinationOutOfReach:
                    OperationFailService.Send(player.CreatureId, TextConstants.DESTINATION_IS_OUT_OF_REACH);
                    break;
                case InvalidOperation.NotEnoughRoom:
                    OperationFailService.Send(player.CreatureId, TextConstants.NOT_ENOUGH_ROOM);
                    break;
                case InvalidOperation.Exhausted:
                    OperationFailService.Send(player.CreatureId, InvalidOperation.Exhausted);
                    break;
                default:
                    OperationFailService.Send(player.CreatureId, TextConstants.NOT_POSSIBLE);
                    break;
            }
            return;
        }

        // Start cooldown for the push action
        player.StartCooldown(CooldownType.PushCreature, 2_000);

        // Perform the actual push based on a creature type
        switch (target)
        {
            case IWalkableCreature walkableTarget when target is IMonster or INpc:
                // Use WalkTo for monsters and NPCs (sends messages)
                walkableTarget.WalkTo(toTile.Location);
                break;
            
            case IPlayer playerTarget:
                // Use direct move for players (no messages sent to them)
                creatureMovementService.MoveCreature(playerTarget, toTile.Location);
                break;
        }
    }
}