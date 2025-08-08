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

        message.AddUInt16(player.GetSkillLevel(SkillType.Fist));
        message.AddUInt16(player.GetSkillLevel(SkillType.Fist)); //todo: 1098 imeplment this base skill
        message.AddByte(player.GetSkillPercent(SkillType.Fist));

        message.AddUInt16(player.GetSkillLevel(SkillType.Club));
        message.AddUInt16(player.GetSkillLevel(SkillType.Club)); //todo: 1098 imeplment this base skill
        message.AddByte(player.GetSkillPercent(SkillType.Club));

        message.AddUInt16(player.GetSkillLevel(SkillType.Sword));
        message.AddUInt16(player.GetSkillLevel(SkillType.Sword)); //todo: 1098 imeplment this base skill
        message.AddByte(player.GetSkillPercent(SkillType.Sword));

        message.AddUInt16(player.GetSkillLevel(SkillType.Axe));
        message.AddUInt16(player.GetSkillLevel(SkillType.Axe)); //todo: 1098 imeplment this base skill
        message.AddByte(player.GetSkillPercent(SkillType.Axe));

        message.AddUInt16(player.GetSkillLevel(SkillType.Distance));
        message.AddUInt16(player.GetSkillLevel(SkillType.Distance)); //todo: 1098 imeplment this base skill
        message.AddByte(player.GetSkillPercent(SkillType.Distance));

        message.AddUInt16(player.GetSkillLevel(SkillType.Shielding));
        message.AddUInt16(player.GetSkillLevel(SkillType.Shielding)); //todo: 1098 imeplment this base skill
        message.AddByte(player.GetSkillPercent(SkillType.Shielding));

        message.AddUInt16(player.GetSkillLevel(SkillType.Fishing));
        message.AddUInt16(player.GetSkillLevel(SkillType.Fishing)); //todo: 1098 imeplment this base skill
        message.AddByte(player.GetSkillPercent(SkillType.Fishing));

        //todo: 1098 impelment special skills
        message.AddUInt16(0); //SpecialSkillType.CriticalHitChance
        message.AddUInt16(0);

        message.AddUInt16(0); //SpecialSkillType.CriticalHitAmount
        message.AddUInt16(0);

        message.AddUInt16(0); //SpecialSkillType.LifeLeechChance
        message.AddUInt16(0);

        message.AddUInt16(0); //SpecialSkillType.LifeLeechAmount
        message.AddUInt16(0);

        message.AddUInt16(0); //SpecialSkillType.ManaLeechChance
        message.AddUInt16(0);

        message.AddUInt16(0); //SpecialSkillType.ManaLeechAmount
        message.AddUInt16(0);
    }
}