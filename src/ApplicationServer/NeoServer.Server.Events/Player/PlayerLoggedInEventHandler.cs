using System;
using NeoServer.Data.Interfaces;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Server.Events.Player;

public class PlayerLoggedInEventHandler : IEventHandler
{
    private readonly IPlayerRepository _playerRepository;

    public PlayerLoggedInEventHandler(IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository;
    }

    public async void Execute(IWalkableCreature creature)
    {
        if (creature.IsNull()) return;

        if (creature is not IPlayer player) return;

        await _playerRepository.UpdatePlayerOnlineStatus(player.Id, true);
        await _playerRepository.UpdateLastLogInDate((int)player.Id, player.LastLogIn ?? DateTime.UtcNow);
    }
}