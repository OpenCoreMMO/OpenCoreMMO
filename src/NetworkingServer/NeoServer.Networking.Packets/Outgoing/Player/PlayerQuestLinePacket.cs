using System.Collections.Generic;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Player;

public class PlayerQuestLinePacket: OutgoingPacket
{
    public required ushort QuestId { get; init; }
    public required byte NumberOfMissions { get; set; }
    public required List<Mission> Missions { get; set; }
    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte((byte)GameOutgoingPacketType.QuestLine);
        message.AddUInt16(QuestId);
        message.AddByte(NumberOfMissions);

        if (Missions is null) return;
        
        foreach (var mission in Missions)
        {
            if (!mission.IsStarted) continue;
            
            message.AddString(mission.Name);
            message.AddString(mission.Description);
        }
    }

    public record Mission(string Name, string Description, bool IsStarted);
}