using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NeoServer.Data.Entities;
using NeoServer.Data.Interfaces;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Loaders.Interfaces;
using NeoServer.Networking.Packets.Incoming.Chat;
using NeoServer.Networking.Packets.Outgoing;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Network;
using NeoServer.Server.Tasks;
using Serilog;

namespace NeoServer.Networking.Handlers.Chat;

public class PlayerAddVipHandler : PacketHandler
{
    private readonly IGameServer _game;
    private readonly ILogger _logger;
    private readonly IEnumerable<IPlayerLoader> _playerLoaders;
    private readonly IPlayerRepository _playerRepository;

    public PlayerAddVipHandler(IGameServer game, IPlayerRepository playerRepository,
        IEnumerable<IPlayerLoader> playerLoaders, ILogger logger)
    {
        _game = game;
        _playerRepository = playerRepository;
        _playerLoaders = playerLoaders;
        _logger = logger;
    }

    public override async void HandleMessage(IReadOnlyNetworkMessage message, IConnection connection)
    {
        if (Guard.AnyNull(connection, message)) return;

        if (!_game.CreatureManager.TryGetPlayer(connection.CreatureId, out var player)) return;

        var addVipPacket = new AddVipPacket(message);

        if (addVipPacket.Name?.Length > 20) return;

        var vipPlayer = await GetVipPlayer(addVipPacket);

        if (vipPlayer is null)
        {
            connection.Send(new TextMessagePacket("A player with this name does not exist.",
                TextMessageOutgoingType.Small));
            return;
        }

        _game.Dispatcher.AddEvent(new Event(() => player.Vip.AddToVip(vipPlayer)));
    }

    private async Task<IPlayer> GetVipPlayer(AddVipPacket addVipPacket)
    {
        if (Guard.IsNull(addVipPacket)) return null;

        //return player if it is already loaded in the game
        if (_game.CreatureManager.TryGetPlayer(addVipPacket.Name, out var vipPlayer)) return vipPlayer;

        var playerRecord = await GetPlayerRecord(addVipPacket);
        if (playerRecord is null) return null;

        if (_playerLoaders.FirstOrDefault(x => x.IsApplicable(playerRecord)) is not { } playerLoader)
            return null;

        vipPlayer = playerLoader.Load(playerRecord);

        return vipPlayer;
    }

    private async Task<PlayerEntity> GetPlayerRecord(AddVipPacket addVipPacket)
    {
        PlayerEntity playerRecord = null;

        try
        {
            playerRecord = await _playerRepository.GetByName(addVipPacket.Name);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Unable to retrieve player record");
        }

        return playerRecord;
    }
}