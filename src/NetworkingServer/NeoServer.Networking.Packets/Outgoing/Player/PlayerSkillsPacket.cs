using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Player;

public class PlayerSkillsPacket : OutgoingPacket
{
    private readonly IPlayer player;

    public PlayerSkillsPacket(IPlayer player)
    {
        this.player = player;
    }

    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte((byte)GameOutgoingPacketType.PlayerSkills);
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