using System;
using NeoServer.Data.Entities;
using NeoServer.Data.Interfaces;
using NeoServer.Domain.Common;
using NeoServer.Networking.Packets.Incoming.Reports;
using NeoServer.Networking.Packets.Outgoing;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Handlers.Reports;

public class PlayerReportBugHandler(
    IReportBugRepository reportBugRepository,
    IGameCreatureManager creatureManager,
    GameConfiguration gameConfiguration) : PacketHandler
{
    public override async void HandleMessage(IReadOnlyNetworkMessage message, IConnection connection)
    {
        var playerReportBug = new PlayerReportBugPacket(message);

        if (!creatureManager.TryGetPlayer(connection.CreatureId, out var player)) return;

        var reportBugEntity = await reportBugRepository.GetLatestReportPendingByPlayerIdAsync(player.Id);

        if (reportBugEntity != null)
        {
            var timeSinceLastReport = DateTime.UtcNow - reportBugEntity.CreatedAt;
            if (timeSinceLastReport.TotalMinutes < gameConfiguration.Report.ReportMaxTime)
            {
                connection.Send(new TextMessagePacket(
                    $"You can only report a bug every {gameConfiguration.Report.ReportMaxTime} minutes.",
                    TextMessageOutgoingType.Small));
                return;
            }
        }

        await reportBugRepository.Insert(new ReportBugEntity
        {
            PlayerId = player.Id,
            Reason = playerReportBug.Reason,
            Ip = connection.Ip,
            PosX = player.Location.X,
            PosY = player.Location.Y,
            PosZ = player.Location.Z,
            CreatedAt = DateTime.UtcNow
        });

        connection.Send(new TextMessagePacket(
            "Thank you for your report. Your report will be processed by the team as soon as possible.",
            TextMessageOutgoingType.Small));
    }
}