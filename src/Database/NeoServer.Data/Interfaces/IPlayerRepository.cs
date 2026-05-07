using System;
using System.Collections.Generic;

using NeoServer.Data.Entities;
using NeoServer.Domain.Common.Contracts.Creatures;

namespace NeoServer.Data.Interfaces;

public interface IPlayerRepository : IBaseRepositoryNeo<PlayerEntity>
{
    void UpdateAllPlayersToOffline();
    List<PlayerOutfitAddonEntity> GetOutfitAddons(int playerId);
    void UpdatePlayers(IEnumerable<IPlayer> players);
    void UpdatePlayerOnlineStatus(uint playerId, bool status);
    PlayerEntity GetByName(string playerName);

    /// <summary>
    ///     Save player info, inventory, backpack and depot
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    void SavePlayer(IPlayer player);

    PlayerEntity GetById(int id);
    void UpdateLastLogInDate(int playerId, DateTime lastLogIn);
}