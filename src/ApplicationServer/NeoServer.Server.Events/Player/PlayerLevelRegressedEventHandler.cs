using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Parsers;
using NeoServer.Networking.Packets.Outgoing;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Server.Events.Player;

public class PlayerLevelRegressedEventHandler : PlayerLevelChangeEventHandler
{
    public PlayerLevelRegressedEventHandler(IGameServer game) : base(game)
    {
    }

    protected override void SendLevelChangeMessage(SkillType skillType, IConnection connection, int fromLevel,
        int toLevel)
    {
        connection.OutgoingPackets.Enqueue(new TextMessagePacket(
            MessageParser.GetSkillRegressedMessage(skillType, fromLevel, toLevel),
            TextMessageOutgoingType.MESSAGE_EVENT_ADVANCE));
    }
}