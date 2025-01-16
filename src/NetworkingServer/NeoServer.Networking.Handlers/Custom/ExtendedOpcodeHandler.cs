using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Network;
using NeoServer.Server.Common.Contracts.Scripts;
using NeoServer.Server.Tasks;

namespace NeoServer.Networking.Handlers.Custom;

public class ExtendedOpcodeHandler : PacketHandler
{
    private readonly IGameServer _game;
    private readonly IScriptGameManager _scriptGameManager;

    public ExtendedOpcodeHandler(IGameServer game, IScriptGameManager scriptGameManager)
    {
        _game = game;
        _scriptGameManager = scriptGameManager;
    }

    public override void HandleMessage(IReadOnlyNetworkMessage message, IConnection connection)
    {
        var opcode = message.GetByte();
        var buffer = message.GetString() ?? string.Empty;

        if (_game.CreatureManager.TryGetPlayer(connection.CreatureId, out var player))
            _game.Dispatcher.AddEvent(new Event(() =>
                _scriptGameManager.CreatureEvents.PlayerExtendedOpcodeHandle(player, opcode, buffer)));
    }
}