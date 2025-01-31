using System;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Creatures;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Player;

public class PlayerStatusPacket(IPlayer player) : OutgoingPacket
{
    public override byte PacketType => (byte)GameOutgoingPacketType.PlayerStatus;

    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte(PacketType);
        message.AddUInt16((ushort)Math.Min(ushort.MaxValue, player.HealthPoints));
        message.AddUInt16((ushort)Math.Min(ushort.MaxValue, player.MaxHealthPoints));
        message.AddUInt32((uint)player.FreeCapacity * 100);

        message.AddUInt32(Math.Min(0x7FFFFFFF,
            player.Experience)); // Experience: Client debugs after 2,147,483,647 exp

        message.AddUInt16(player.Level);
        message.AddByte(player.LevelPercent);
        message.AddUInt16(Math.Min(ushort.MaxValue, player.Mana));
        message.AddUInt16(Math.Min(ushort.MaxValue, player.MaxMana));
        message.AddByte((byte)player.GetSkillLevel(SkillType.Magic));
        message.AddByte(player.GetSkillPercent(SkillType.Magic));

        message.AddByte(player.SoulPoints);
        message.AddUInt16(player.StaminaMinutes);
    }
}