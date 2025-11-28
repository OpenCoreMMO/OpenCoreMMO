using NeoServer.Networking.Packets.Outgoing;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Handlers.Reports;

public class PlayerReportRuleViolationHandler(
    IGameCreatureManager creatureManager) : PacketHandler
{
    public override void HandleMessage(IReadOnlyNetworkMessage message, IConnection connection)
    {
        if (!creatureManager.TryGetPlayer(connection.CreatureId, out _)) return;

        connection.Send(new TextMessagePacket(
            "Thank you for your report. Your report will be processed by the team as soon as possible.",
            TextMessageOutgoingType.Small));
    }
}