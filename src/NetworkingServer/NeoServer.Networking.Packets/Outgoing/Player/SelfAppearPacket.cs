using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Server.Common.Contracts.Network;
using System;

namespace NeoServer.Networking.Packets.Outgoing.Player;

public class SelfAppearPacket : OutgoingPacket
{
    private readonly IPlayer player;

    public SelfAppearPacket(IPlayer player)
    {
        this.player = player;
    }

    private byte GraphicsSpeed => 0x32; //  beat duration (50)
    private byte CanReportBugs => 0x00;

    private double SpeedA = 857.36;
    private double SpeedB = 261.29;
    private double SpeedC = -4795.01;

    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte((byte)GameOutgoingPacketType.SelfAppear);

        message.AddUInt32(player.CreatureId);
        message.AddUInt16(GraphicsSpeed);

        //todo: 1098 implement this

        message.AddDouble(SpeedA, 3);
        message.AddDouble(SpeedB, 3);
        message.AddDouble(SpeedC, 3);

        message.AddByte(CanReportBugs); // can report bugs? todo: create tutor account type


        message.AddByte(0x00); // can change pvp framing option
        message.AddByte(0x00); // expert mode button enabled

        message.AddUInt16(0x00); // URL (string) to ingame store images
        message.AddUInt16(25); // premium coin package size

        //todo: 1098 implement this
        message.AddByte(0x0A); //sendPendingStateEntered
    }
}