namespace NeoServer.E2E.Tests.Client;

public sealed record ReceivedPacket(byte Opcode, byte[] Payload);
