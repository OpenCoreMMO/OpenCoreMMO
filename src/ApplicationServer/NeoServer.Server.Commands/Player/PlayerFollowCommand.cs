using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Server.Common.Contracts.Commands;

namespace NeoServer.Server.Commands.Player;

public class PlayerFollowCommand : ICommand
{
    public void Execute(IPlayer player, ICreature target)
    {
        player.StopAttack();
        player.Follow(target);
    }
}