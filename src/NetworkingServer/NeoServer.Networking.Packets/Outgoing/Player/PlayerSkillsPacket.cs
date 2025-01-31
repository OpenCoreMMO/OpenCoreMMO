using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Creatures;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Player;

public class PlayerSkillsPacket(IPlayer player) : OutgoingPacket
{
    public override byte PacketType => (byte)GameOutgoingPacketType.PlayerSkills;

    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte(PacketType);

        message.AddByte((byte)player.GetSkillLevel(SkillType.Fist));
        message.AddByte(player.GetSkillPercent(SkillType.Fist));

        message.AddByte((byte)player.GetSkillLevel(SkillType.Club));
        message.AddByte(player.GetSkillPercent(SkillType.Club));

        message.AddByte((byte)player.GetSkillLevel(SkillType.Sword));
        message.AddByte(player.GetSkillPercent(SkillType.Sword));

        message.AddByte((byte)player.GetSkillLevel(SkillType.Axe));
        message.AddByte(player.GetSkillPercent(SkillType.Axe));

        message.AddByte((byte)player.GetSkillLevel(SkillType.Distance));
        message.AddByte(player.GetSkillPercent(SkillType.Distance));

        message.AddByte((byte)player.GetSkillLevel(SkillType.Shielding));
        message.AddByte(player.GetSkillPercent(SkillType.Shielding));

        message.AddByte((byte)player.GetSkillLevel(SkillType.Fishing));
        message.AddByte(player.GetSkillPercent(SkillType.Fishing));
    }
}