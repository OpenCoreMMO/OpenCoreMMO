using System;
using System.Linq;
using System.Threading.Tasks;
using NeoServer.Data.Interfaces;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Creatures.Services;
using NeoServer.Loaders.Guilds;
using NeoServer.Loaders.Interfaces;
using NeoServer.Networking.Packets.Outgoing.Custom;
using NeoServer.Networking.Packets.Outgoing.Login;
using NeoServer.Server.Commands.WaitingInLine;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Commands;
using NeoServer.Server.Common.Contracts.Network;
using NeoServer.Server.Common.Contracts.Scripts;
using NeoServer.Server.Common.Enums;
using NeoServer.Server.Configurations;
using NeoServer.Server.Services;
using Serilog;
using OperatingSystem = NeoServer.Server.Common.Enums.OperatingSystem;

namespace NeoServer.Server.Commands.Player;

public class PlayerLogInCommand(
    IGameServer game,
    IPlayerLoader playerLoader,
    GuildLoader guildLoader,
    PlayerLocationResolver playerLocationResolver,
    ILogger logger,
    IScriptManager scriptManager,
    IPlayerRepository playerRepository,
    PlayerChannelService playerChannelService,
    IMap map,
    IAccountRepository accountRepository,
    ServerConfiguration serverConfiguration,
    ClientConfiguration clientConfiguration,
    IIpBansRepository ipBansRepository,
    IWaitingQueueManager waitingQueueManager)
    : ICommand
{
    public async Task<(bool Success, string Message)> Execute(PlayerLogInRequest request, IConnection connection)
    {
        connection.SetXtea(request.Xtea);

        if (request.ChallengeTimeStamp != connection.TimeStamp || request.ChallengeNumber != connection.RandomNumber)
            return (false, "Login challenge is not valid.");

        if (string.IsNullOrWhiteSpace(request.Account))
            return (false, "You must enter your account name.");

        if (request.Version < serverConfiguration.MinVersion || request.Version > serverConfiguration.Version)
            return (false, $"Only clients with protocol {serverConfiguration.Version} allowed!");

        switch (game.State)
        {
            case GameState.Opening:
                return (false, "Gameworld is starting up. Please wait.");
            case GameState.Maintaining:
                return (false, "Gameworld is under maintenance. Please re-connect in a while.");
            case GameState.Closed:
                return (false, "Server is currently closed. Please try again later.");
        }

        var existBan = await ipBansRepository.ExistBan(connection.Ip.Split(":")[0]);
        if (existBan is not null)
            return (false,
                $"Your IP address {existBan.Ip} has been banished until {existBan.ExpiresAt:MM/dd/yyyy}.\nReason: {existBan.Reason}");

        var playersOnline = await accountRepository.GetOnlinePlayers(request.Account);

        foreach (var playerOnline in playersOnline)
        {
            game.CreatureManager.TryGetLoggedPlayer((uint)playerOnline.Id, out var existingPlayer);
            if (existingPlayer?.Name == request.CharacterName)
            {
                game.CreatureManager.GetPlayerConnection(existingPlayer.CreatureId, out var existingConnection);
                existingConnection?.Disconnect();
                logger.Warning("Player {PlayerName} logged out because of another login attempt", existingPlayer.Name);
            }
            else if (!playerOnline.Account.AllowManyOnline)
            {
                return (false, "You may only login with one character of your account at the same time.");
            }
        }

        var playerRecord = await accountRepository.GetPlayer(request.Account, request.Password, request.CharacterName,
            includeKillsLastMonth: true);
        if (playerRecord is null)
            return (false, "Account name or password is not correct.");

        if (playerRecord.Account.BanishedAt is not null)
            return (false, "Your account is banned.");

        if (!waitingQueueManager.CanLogin(playerRecord, out var currentSlot))
        {
            var retryTime = waitingQueueManager.GetTime(currentSlot);
            var message = $"There are too many players online.\nYou are at place {currentSlot} on waiting list.";
            var waitingInLinePacket = new WaitingInLinePacket(message, retryTime);
            connection.Send(waitingInLinePacket);
            connection.Close();
            return (false, message);
        }

        connection.OtcV8Version = request.OtcV8Version;
        if (request.OtcV8Version > 0 || request.OperatingSystem >= OperatingSystem.OtcLinux)
        {
            if (request.OtcV8Version > 0)
                connection.Send(new FeaturesPacket
                {
                    GameEnvironmentEffect = clientConfiguration.OtcV8.GameEnvironmentEffect,
                    GameExtendedOpcode = clientConfiguration.OtcV8.GameExtendedOpcode,
                    GameExtendedClientPing = clientConfiguration.OtcV8.GameExtendedClientPing,
                    GameItemTooltip = clientConfiguration.OtcV8.GameItemTooltip
                });
            connection.Send(new OpcodeMessagePacket());
        }

        var playerAlreadyLoggedIn = game.CreatureManager.TryGetLoggedPlayer((uint)playerRecord.Id, out var player);

        if (!playerAlreadyLoggedIn)
        {
            await guildLoader.LoadAsync(playerRecord.GuildMember?.Guild);

            var playerLocation = playerLocationResolver.GetPlayerLocation(playerRecord);
            if (playerLocation == Location.Zero) return (false, "Player location invalid");

            playerRecord.PosX = playerLocation.X;
            playerRecord.PosY = playerLocation.Y;
            playerRecord.PosZ = playerLocation.Z;

            player = playerLoader.Load(playerRecord);
        }

        game.CreatureManager.AddPlayer(player, connection);

        //player must be placed on map before login to avoid issues with map description packet
        map.PlaceCreature(player);

        player.Login();
        player.Vip.LoadVipList(playerRecord.Account.VipList.Select(x => ((uint)x.PlayerId, x.Player?.Name)));

        playerChannelService.JoinChannels(player);

        logger.Information("Player {PlayerName} logged in", player.Name);

        var (success, current, old) = await game.CreatureManager.CheckPlayersRecord(player.WorldId);

        if (success)
            scriptManager.GlobalEvents.ExecuteRecord(current, old);

        await playerRepository.UpdatePlayerOnlineStatus(player.Id, true);
        await playerRepository.UpdateLastLogInDate((int)player.Id, player.LastLogIn ?? DateTime.UtcNow);

        return (true, null);
    }
}