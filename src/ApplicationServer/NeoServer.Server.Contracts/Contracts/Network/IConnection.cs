using System;
using System.Collections.Generic;
using NeoServer.Game.Common.Contracts.Creatures;

namespace NeoServer.Server.Common.Contracts.Network;

public interface IConnection
{
    IReadOnlyNetworkMessage InMessage { get; }
    uint[] XteaKey { get; }
    uint CreatureId { get; }
    bool IsAuthenticated { get; }
    bool Disconnected { get; }
    Queue<IOutgoingPacket> OutgoingPackets { get; }
    long LastPingRequest { get; set; }
    long LastPingResponse { get; set; }
    string Ip { get; }
    long TimeStamp { get; }
    byte RandomNumber { get; }

    event EventHandler<IConnectionEventArgs> OnProcessEvent;
    event EventHandler<IConnectionEventArgs> OnCloseEvent;
    event EventHandler<IConnectionEventArgs> OnPostProcessEvent;

    void BeginStreamRead();
    void Close(bool force = false);
    void Disconnect(string text = null);
    public void Send(IOutgoingPacket packet);
    void SendFirstConnection();
    void SetXtea(uint[] xtea);
    void SetAsAuthenticated();
    void SetConnectionOwner(IPlayer player);
    void Send();
}