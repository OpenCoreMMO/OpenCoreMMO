using System.Collections.Generic;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Player;

public class PlayerQuestLogPacket: OutgoingPacket
{
    public required ushort NumberOfQuests { get; init; }
    public required List<Quest> Quests { get; init; }
    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte((byte)GameOutgoingPacketType.QuestLog);
        message.AddUInt16(NumberOfQuests);
        if (Quests is null) return;

        foreach (var quest in Quests)
        {
            if (!quest.IsStarted) continue;
            
            message.AddUInt16(quest.Id);
            message.AddString(quest.Name);
            message.AddByte(quest.IsCompleted ? (byte)1 : (byte)0);
        }
    }

    public record Quest(ushort Id,  string Name, bool IsStarted, bool IsCompleted);
}