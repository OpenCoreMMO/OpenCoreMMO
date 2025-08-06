using System;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Player;

public class PlayerStatusPacket : OutgoingPacket
{
    private readonly IPlayer player;

    public PlayerStatusPacket(IPlayer player)
    {
        this.player = player;
    }

    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte((byte)GameOutgoingPacketType.PlayerStatus);

        message.AddUInt16((ushort)Math.Min(ushort.MaxValue, player.HealthPoints));
        message.AddUInt16((ushort)Math.Min(ushort.MaxValue, player.MaxHealthPoints));

        message.AddUInt32((uint)player.FreeCapacity * 100);
        message.AddUInt32((uint)player.TotalCapacity);

        message.AddUInt64(player.Experience);

        message.AddUInt16(player.Level);
        message.AddByte(player.LevelPercent);


        message.AddUInt16(100); // base xp gain rate
        message.AddUInt16(0); // xp voucher
        message.AddUInt16(0); // low level bonus
        message.AddUInt16(0); // xp boost
        message.AddUInt16(100); // stamina multiplier (100 = x1.0)

        message.AddUInt16((ushort)Math.Min(ushort.MaxValue, player.Mana));
        message.AddUInt16((ushort)Math.Min(ushort.MaxValue, player.MaxMana));

        message.AddByte((byte)player.GetSkillLevel(SkillType.Magic));
        message.AddByte((byte)player.GetSkillLevel(SkillType.Magic)); //todo: implement this

        message.AddByte(player.GetSkillPercent(SkillType.Magic));

        message.AddByte(player.SoulPoints);

        message.AddUInt16(player.StaminaMinutes);

        message.AddUInt16((ushort)(player.Speed / 2));
        //todo: 1098 implement this
        message.AddUInt16(0x00);//todo: condition_regeneration
        //todo: 1098 implement this
        message.AddUInt16(0);//todo: offline trainig

        message.AddUInt16(0);// xp boost time (seconds)
        message.AddByte(0); // enables exp boost in the store
    }
}