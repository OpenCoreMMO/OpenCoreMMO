using NeoServer.Data.Entities;
using NeoServer.Networking.Packets.Incoming;
using NeoServer.Server.Common.Contracts.Network;
using System;

namespace NeoServer.Networking.Packets.Outgoing.Login;

public class CharacterListPacket : OutgoingPacket
{
    private readonly AccountEntity _accountEntity;
    private readonly string _ipAddress;
    private readonly ushort _port;
    private readonly string _serverName;
    private readonly AccountLoginPacket _accountLoginPacket;

    public CharacterListPacket(AccountEntity account, string serverName, string ipAddress, ushort port, AccountLoginPacket accountLoginPacket)
    {
        _accountEntity = account;
        _serverName = serverName;
        _ipAddress = ipAddress;
        _port = port;
        _accountLoginPacket = accountLoginPacket;
    }

    public override void WriteToMessage(INetworkMessage message)
    {
        AddCharList(message);
    }

    private void AddCharList(INetworkMessage message)
    {
        //Token Autenticador
        const uint AUTHENTICATOR_PERIOD = 30;
        uint ticks = (uint)(DateTimeOffset.UtcNow.ToUnixTimeSeconds() / AUTHENTICATOR_PERIOD);

        if (!string.IsNullOrEmpty(_accountEntity.Secret))
        {
            bool validToken = false;
            string token = _accountLoginPacket.Token ?? string.Empty;

            //todo: 1098 implement this

            string GenerateToken(string key, uint t) => key + t.ToString(); // todo: generate a real token

            if (!string.IsNullOrEmpty(token))
            {
                validToken =
                    token == GenerateToken(_accountEntity.Secret, ticks) ||
                    token == GenerateToken(_accountEntity.Secret, ticks - 1) ||
                    token == GenerateToken(_accountEntity.Secret, ticks + 1);
            }

            if (!validToken)
            {
                message.AddByte(0x0D);
                message.AddByte(0);
                return;
            }

            message.AddByte(0x0C);
            message.AddByte(0);
        }

        //Add MOTD
        string motd = GetMotd();
        int motdNumber = GetMotdNumber();

        if (!string.IsNullOrEmpty(motd))
        {
            message.AddByte(0x14);
            message.AddString($"{motdNumber}\n{motd}");
        }

        //Add session key
        message.AddByte(0x28);
        message.AddString($"{_accountLoginPacket.Account}\n{_accountLoginPacket.Password}\n{_accountLoginPacket.Token}\n{ticks}");

        //Add char list
        message.AddByte(0x64);

        message.AddByte(1); // number of worlds
        message.AddByte(0); // world id
        message.AddString(_serverName);
        message.AddString(_ipAddress);
        message.AddUInt16(_port);
        message.AddByte(0);

        byte size = (byte)Math.Min(byte.MaxValue, _accountEntity.Players.Count);
        message.AddByte(size);

        foreach (var player in _accountEntity.Players)
        {
            message.AddByte(0); // 0 = offline, 1 = online
            message.AddString(player.Name);
        }

        //Add premium days
        message.AddByte(0); // ?
        bool hasPremium = _accountEntity.PremiumTimeEndAt.HasValue && _accountEntity.PremiumTimeEndAt.Value > DateTime.UtcNow;
        message.AddByte(hasPremium ? (byte)1 : (byte)0);
        uint premiumEndsAt = hasPremium
            ? (uint)((DateTimeOffset)_accountEntity.PremiumTimeEndAt.Value).ToUnixTimeSeconds()
            : 0;
        message.AddUInt32(premiumEndsAt);
    }

    //todo: 1098 implement this
    private string GetMotd() => "Bem-vindo ao servidor!";
    //todo: 1098 implement this
    private int GetMotdNumber() => 1;
}