using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Player;

public class SelfAppearPacket(IPlayer player) : OutgoingPacket
{
    private byte GraphicsSpeed => 0x32; //  beat duration (50)
    private byte CanReportBugs => 0x00;

    public override byte PacketType => (byte)GameOutgoingPacketType.SelfAppear;

    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte(PacketType);
        message.AddUInt32(player.CreatureId);
        message.AddUInt16(GraphicsSpeed);
        message.AddByte(CanReportBugs); // can report bugs? todo: create tutor account type
    }
}