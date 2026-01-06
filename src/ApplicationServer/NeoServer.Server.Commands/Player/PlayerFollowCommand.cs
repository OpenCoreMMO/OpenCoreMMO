using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Server.Common.Contracts.Commands;

namespace NeoServer.Server.Commands.Player;

public abstract class PlayerFollowCommand : ICommand
{
    public void Execute(IPlayer player, ICreature target)
    {
        player.StopAttack();
        player.Follow(target);
    }
}