using NeoServer.Game.Common.Creatures.Players;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Incoming;

public class ChangeModePacket : IncomingPacket
{
    public ChangeModePacket(IReadOnlyNetworkMessage message)
    {
        FightMode = (FightMode)message.GetByte();
        ChaseMode = (ChaseMode)message.GetByte();
        SecureMode = (PvpSecureMode) message.GetByte();
    }

    public FightMode FightMode { get; }
    public ChaseMode ChaseMode { get; }
    public PvpSecureMode SecureMode { get; }
}