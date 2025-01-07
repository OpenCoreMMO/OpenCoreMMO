using System.Linq;
using NeoServer.Data.Entities;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Game.Common.Results;
using NeoServer.Loaders.Guilds;
using NeoServer.Loaders.Interfaces;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Commands;
using NeoServer.Server.Common.Contracts.Network;
using NeoServer.Server.Common.Contracts.Scripts;
using NeoServer.Server.Services;
using Serilog;

namespace NeoServer.Server.Commands.Player;

public class PlayerLogInCommand : ICommand
{
    private readonly ILogger _logger;
    private readonly PlayerLocationResolver _playerLocationResolver;
    private readonly IGameServer _game;
    private readonly GuildLoader _guildLoader;
    private readonly IPlayerLoader _playerLoader;
    private readonly IScriptGameManager _scriptGameManager;

    public PlayerLogInCommand(
        IGameServer game,
        IPlayerLoader playerLoader,
        GuildLoader guildLoader,
        PlayerLocationResolver playerLocationResolver,
        ILogger logger,
        IScriptGameManager scriptGameManager)
    {
        _game = game;
        _playerLoader = playerLoader;
        _guildLoader = guildLoader;
        _playerLocationResolver = playerLocationResolver;
        _logger = logger;
        _scriptGameManager = scriptGameManager;
    }

    public Result Execute(PlayerEntity playerRecord, IConnection connection)
    {
        if (playerRecord is null)
            //todo validations here
            return Result.Fail(InvalidOperation.PlayerNotFound);

        if (!_game.CreatureManager.TryGetLoggedPlayer((uint)playerRecord.Id, out var player))
        {
            _guildLoader.Load(playerRecord.GuildMember?.Guild);

            var playerLocation = _playerLocationResolver.GetPlayerLocation(playerRecord);
            if (playerLocation == Location.Zero) return Result.Fail(InvalidOperation.PlayerLocationInvalid);

            playerRecord.PosX = playerLocation.X;
            playerRecord.PosY = playerLocation.Y;
            playerRecord.PosZ = playerLocation.Z;

            player = _playerLoader.Load(playerRecord);
        }

        _game.CreatureManager.AddPlayer(player, connection);

        player.Login();
        player.Vip.LoadVipList(playerRecord.Account.VipList.Select(x => ((uint)x.PlayerId, x.Player?.Name)));
        _logger.Information("Player {PlayerName} logged in", player.Name);

        (var success, var current, var old) = _game.CreatureManager.CheckPlayersRecord(player.WorldId).Result;

        if (success)
            _scriptGameManager.GlobalEventExecuteRecord(current, old);

        return Result.Success;
    }
}