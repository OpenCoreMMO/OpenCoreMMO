using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Server.Common.Contracts.Commands;

namespace NeoServer.Server.Commands.Player;

public class PlayerFollowCommand : ICommand
{
    public void Execute(IPlayer player, ICreature target)
    {
        if (target is null)
        {
            player.StopFollowing();
            return;
        }
        
        player.StopAttack();
        player.Follow(target);
    }
}