using System;
using System.Linq;
using NeoServer.Data.Entities;
using NeoServer.Data.Interfaces;
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Common.Results;
using NeoServer.Domain.Creatures.Services;
using NeoServer.Loaders.Guilds;
using NeoServer.Loaders.Interfaces;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Commands;
using NeoServer.Server.Common.Contracts.Network;
using NeoServer.Server.Common.Contracts.Scripts;
using NeoServer.Server.Services;
using Serilog;

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
    IMap map)
    : ICommand
{
    public Result Execute(PlayerEntity playerRecord, IConnection connection)
    {
        if (playerRecord is null)
        {
            //todo validations here
            return Result.Fail(InvalidOperation.PlayerNotFound);
        }

        if (!game.CreatureManager.TryGetLoggedPlayer((uint)playerRecord.Id, out var player))
        {
            guildLoader.Load(playerRecord.GuildMember?.Guild);

            var playerLocation = playerLocationResolver.GetPlayerLocation(playerRecord);
            if (playerLocation == Location.Zero) return Result.Fail(InvalidOperation.PlayerLocationInvalid);

            playerRecord.PosX = playerLocation.X;
            playerRecord.PosY = playerLocation.Y;
            playerRecord.PosZ = playerLocation.Z;

            player = playerLoader.Load(playerRecord);
        }

        game.CreatureManager.AddPlayer(player, connection);

        player.Login();
        player.Vip.LoadVipList(playerRecord.Account.VipList.Select(x => ((uint)x.PlayerId, x.Player?.Name)));
      
        map.PlaceCreature(player);

        playerChannelService.JoinChannels(player);
        
        logger.Information("Player {PlayerName} logged in", player.Name);

        var (success, current, old) = game.CreatureManager.CheckPlayersRecord(player.WorldId).Result;

        if (success)
            scriptManager.GlobalEvents.ExecuteRecord(current, old);
        
        playerRepository.UpdatePlayerOnlineStatus(player.Id, true).Wait();
        playerRepository.UpdateLastLogInDate((int)player.Id, player.LastLogIn ?? DateTime.UtcNow).Wait();

        return Result.Success;
    }
}