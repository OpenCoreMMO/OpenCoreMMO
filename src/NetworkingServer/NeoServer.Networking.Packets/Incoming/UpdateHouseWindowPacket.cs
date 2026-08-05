using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Incoming;

public class UpdateHouseWindowPacket : IncomingPacket
{
    public UpdateHouseWindowPacket(IReadOnlyNetworkMessage message)
    {
        DoorId = message.GetByte();
        WindowTextId = message.GetUInt32();
        Text = message.GetString();
    }

    public byte DoorId { get; }
    public uint WindowTextId { get; }
    public string Text { get; }
}
