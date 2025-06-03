using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Effect;

public class MagicEffectPacket : OutgoingPacket
{
    private readonly EffectT effect;
    private readonly Location location;

    public MagicEffectPacket(Location location, EffectT effect)
    {
        this.location = location;
        this.effect = effect;
    }

    public override void WriteToMessage(INetworkMessage message)
    {
        if (effect is 0 or (EffectT)byte.MaxValue) return;

        message.AddByte((byte)GameOutgoingPacketType.MagicEffect);
        message.AddLocation(location);
        message.AddByte((byte)effect);
    }
}