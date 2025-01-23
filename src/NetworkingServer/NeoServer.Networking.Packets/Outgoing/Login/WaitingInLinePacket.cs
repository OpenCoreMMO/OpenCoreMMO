using System;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Login;

public class WaitingInLinePacket : OutgoingPacket
{
    private readonly string _message;
    private readonly long _retryTime;

    public WaitingInLinePacket(string message, long retryTime)
    {
        _message = message;
        _retryTime = retryTime;
    }
    
    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte(0x16);
        message.AddString(_message);
        message.AddByte((byte)_retryTime);
    }
}