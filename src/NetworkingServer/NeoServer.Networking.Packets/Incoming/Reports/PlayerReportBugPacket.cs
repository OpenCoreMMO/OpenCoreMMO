using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Incoming.Reports;

public class PlayerReportBugPacket : IncomingPacket
{
    public PlayerReportBugPacket(IReadOnlyNetworkMessage message)
    {
        Reason = message.GetString();
    }

    public string Reason { get; }
}