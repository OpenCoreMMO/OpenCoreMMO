using NeoServer.Data.Interfaces;
using NeoServer.Domain.Common;
using NeoServer.Networking.Packets.Outgoing;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Handlers.Reports;

public class PlayerReportRuleViolationHandler(IReportBugRepository reportBugRepository, IGameCreatureManager creatureManager, GameConfiguration gameConfiguration) : PacketHandler
{
    public override async void HandleMessage(IReadOnlyNetworkMessage message, IConnection connection)
    {
        if (!creatureManager.TryGetPlayer(connection.CreatureId, out var player)) return;
        
        connection.Send(new TextMessagePacket("Thank you for your report. Your report will be processed by the team as soon as possible.",
            TextMessageOutgoingType.Small));
    }
}
