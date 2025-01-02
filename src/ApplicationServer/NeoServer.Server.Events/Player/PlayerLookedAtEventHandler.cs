using System.Collections.Generic;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Inspection;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Networking.Packets.Outgoing;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Server.Events.Player;

public class PlayerLookedAtEventHandler
{
    private readonly IGameServer _game;
    private readonly IEnumerable<IInspectionTextBuilder> _inspectionTextBuilders;

    public PlayerLookedAtEventHandler(IGameServer game, IEnumerable<IInspectionTextBuilder> inspectionTextBuilders)
    {
        _game = game;
        _inspectionTextBuilders = inspectionTextBuilders;
    }

    public void Execute(IPlayer player, IThing thing, bool isClose)
    {
        if (_game.CreatureManager.GetPlayerConnection(player.CreatureId, out var connection) is false) return;

        var inspectionTextBuilder = GetInspectionTextBuilder(thing);

        var text = thing.GetLookText(isClose, player.CanSeeInspectionDetails);

        connection.OutgoingPackets.Enqueue(new TextMessagePacket(text, TextMessageOutgoingType.Description));
        connection.Send();
    }

    private IInspectionTextBuilder GetInspectionTextBuilder(IThing thing)
    {
        foreach (var inspectionTextBuilder in _inspectionTextBuilders)
            if (inspectionTextBuilder.IsApplicable(thing))
                return inspectionTextBuilder;

        return null;
    }
}