using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Network;
using NeoServer.Server.Common.Contracts.Scripts;
using NeoServer.Server.Tasks;

namespace NeoServer.Networking.Handlers.Custom;

public class ExtendedOpcodeHandler : PacketHandler
{
    private readonly IGameServer _game;
    private readonly IScriptManager _scriptManager;

    public ExtendedOpcodeHandler(IGameServer game, IScriptManager scriptManager)
    {
        _game = game;
        _scriptManager = scriptManager;
    }

    public override void HandleMessage(IReadOnlyNetworkMessage message, IConnection connection)
    {
        var opcode = message.GetByte();
        var buffer = message.GetString() ?? string.Empty;

        if (_game.CreatureManager.TryGetPlayer(connection.CreatureId, out var player))
            _game.Dispatcher.AddEvent(new Event(() =>
                _scriptManager.CreatureEvents.ExtendedOpcodeHandle(player, opcode, buffer)));
    }
}