using System;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Location.Structs;

namespace NeoServer.Server.Common.Contracts.Network;

public interface INetworkMessage : IReadOnlyNetworkMessage
{
    void AddByte(byte b);
    void AddBytes(ReadOnlySpan<byte> bytes);
    void AddPaddingBytes(int count);
    void AddString(string value);
    void AddUInt16(ushort value);
    void AddUInt32(uint value);
    void WriteUint32(uint value, int position);
    byte[] AddHeader(bool addChecksum = true);
    void AddItem(IItem item);
    void AddLocation(Location location);
    void AddLength();
}