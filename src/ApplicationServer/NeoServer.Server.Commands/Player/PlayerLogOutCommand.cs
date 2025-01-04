using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Commands;

namespace NeoServer.Server.Commands.Player;

public class PlayerLogOutCommand : ICommand
{
    private readonly IGameServer game;

    public PlayerLogOutCommand(IGameServer game)
    {
        this.game = game;
    }

    public void Execute(IPlayer player, bool forced = false)
    {
        if (!player.Logout(forced) && !forced) return;

        game.CreatureManager.RemovePlayer(player);
    }
}