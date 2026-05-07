using System.Linq;
using NeoServer.Data.Contexts;
using NeoServer.Data.Entities;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Helpers;

namespace NeoServer.Data.Repositories.Player;

internal static class StorageManager
{
    public static void SaveStorages(IPlayer player, NeoContext neoContext)
    {
        if (Guard.AnyNull(player, player.Storages)) return;

        if (!player.Storages.Any()) return;

        neoContext.PlayerStorages.RemoveRange(neoContext.PlayerStorages.Where(x => x.PlayerId == player.Id));

        foreach (var storage in player.Storages)
        {
            var playerStorage = new PlayerStorageEntity
            {
                PlayerId = (int)player.Id,
                Key = storage.Key,
                Value = storage.Value
            };

            neoContext.Add(playerStorage);
        }
    }
}