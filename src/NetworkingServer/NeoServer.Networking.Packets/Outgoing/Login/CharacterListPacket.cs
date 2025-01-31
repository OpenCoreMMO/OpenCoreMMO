using System;
using NeoServer.Data.Entities;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Login;

public class CharacterListPacket(AccountEntity account, string serverName, string ipAddress, int port) : OutgoingPacket
{
    //todo: muniz refact this
    public override byte PacketType => (byte)LoginOutgoingPacketType.CharacterList;

    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte(PacketType);
        AddCharList(message);
    }

    private void AddCharList(INetworkMessage message)
    {
        message.AddByte((byte)account.Players.Count);

        var parsedIpAddress = ParseIpAddress(ipAddress);

        foreach (var player in account.Players)
        {
            port = player.World?.Port > 0 ? player.World.Port : port;
            if (!string.IsNullOrWhiteSpace(player.World?.Ip))
                parsedIpAddress = ParseIpAddress(player.World.Ip);

            message.AddString(player.Name);
            message.AddString(player.World?.Name ?? serverName ?? string.Empty);

            message.AddByte(parsedIpAddress[0]);
            message.AddByte(parsedIpAddress[1]);
            message.AddByte(parsedIpAddress[2]);
            message.AddByte(parsedIpAddress[3]);

            message.AddUInt16((ushort)port);
        }

        var premiumTimeDays = (ushort) (account.PremiumTimeEndAt is null ? 0 : (account.PremiumTimeEndAt.Value- DateTime.Now).TotalDays);
        message.AddUInt16(premiumTimeDays);
    }

    private static byte[] ParseIpAddress(string ip)
    {
        var localhost = new byte[] { 127, 0, 0, 1 };

        if (string.IsNullOrEmpty(ip)) return localhost;

        var parsedIp = new byte[4];

        var numbers = ip.Split(".");

        if (numbers.Length != 4) return localhost;

        var i = 0;

        foreach (var number in numbers)
        {
            if (!byte.TryParse(numbers[i], out var ipNumber)) return localhost;
            parsedIp[i++] = ipNumber;
        }

        return parsedIp;
    }
}