using System.Collections.Generic;
using System.Linq;
using NeoServer.Data.Entities;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Contracts.World.Tiles;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Game.Common.Results;
using NeoServer.Game.Creatures;
using NeoServer.Game.World;
using NeoServer.Loaders.Guilds;
using NeoServer.Loaders.Interfaces;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Commands;
using NeoServer.Server.Common.Contracts.Network;
using NeoServer.Server.Services;
using Serilog;

namespace NeoServer.Server.Commands.Player;

public class PlayerLogInCommand : ICommand
{
    private readonly ILogger _logger;
    private readonly IGameServer game;
    private readonly GuildLoader guildLoader;
    private readonly PlayerLocationResolver _playerLocationResolver;
    private readonly IEnumerable<IPlayerLoader> playerLoaders;

    public PlayerLogInCommand(IGameServer game, IEnumerable<IPlayerLoader> playerLoaders, GuildLoader guildLoader,
        PlayerLocationResolver playerLocationResolver,
        ILogger logger)
    {
        this.game = game;
        this.playerLoaders = playerLoaders;
        this.guildLoader = guildLoader;
        _playerLocationResolver = playerLocationResolver;
        _logger = logger;
    }

    public Result Execute(PlayerEntity playerRecord, IConnection connection)
    {
        if (playerRecord is null)
            //todo validations here
            return Result.Fail(InvalidOperation.PlayerNotFound);
      
        if (!game.CreatureManager.TryGetLoggedPlayer((uint)playerRecord.Id, out var player))
        {
            if (playerLoaders.FirstOrDefault(x => x.IsApplicable(playerRecord)) is not { } playerLoader)
                return Result.Fail(InvalidOperation.InvalidPlayer);

            guildLoader.Load(playerRecord.GuildMember?.Guild);

            var playerLocation = _playerLocationResolver.GetPlayerLocation(playerRecord);
            if (playerLocation == Location.Zero) return Result.Fail(InvalidOperation.PlayerLocationInvalid);
            
            playerRecord.PosX = playerLocation.X;
            playerRecord.PosY = playerLocation.Y;
            playerRecord.PosZ = playerLocation.Z;
            
            player = playerLoader.Load(playerRecord);
        }

        game.CreatureManager.AddPlayer(player, connection);

        player.Login();
        player.Vip.LoadVipList(playerRecord.Account.VipList.Select(x => ((uint)x.PlayerId, x.Player?.Name)));
        _logger.Information("Player {PlayerName} logged in", player.Name);
        
        return Result.Success;
    }
}