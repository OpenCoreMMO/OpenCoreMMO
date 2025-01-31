namespace NeoServer.Networking.Packets.Outgoing;

public enum LoginOutgoingPacketType : byte
{
    NoType = 0x00,
    LoginFailed = 0x0A,
    Waiting = 0x16,
    CharacterList = 0x64,
}