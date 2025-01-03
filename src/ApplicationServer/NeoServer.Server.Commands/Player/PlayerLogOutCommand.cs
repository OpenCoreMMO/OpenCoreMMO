using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Commands;
using NeoServer.Server.Common.Contracts.Scripts;

namespace NeoServer.Server.Commands.Player;

public class PlayerLogOutCommand : ICommand
{
    private readonly IGameServer game;
    private readonly IScriptGameManager _scriptGameManager;

    public PlayerLogOutCommand(IGameServer game,
        IScriptGameManager scriptGameManager)
    {
        this.game = game;
        _scriptGameManager = scriptGameManager;
    }

    public void Execute(IPlayer player, bool forced = false)
    {
        if (!player.Logout(forced) && !forced) return;

        game.CreatureManager.RemovePlayer(player);
        _scriptGameManager.CreatureEventExecuteOnPlayerLogout(player);
    }
}