using System.Linq;
using System.Threading.Tasks;
using NeoServer.Data.Contexts;
using NeoServer.Data.Entities;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Helpers;

namespace NeoServer.Data.Repositories.Player;

internal static class StorageManager
{
    public static async Task SaveStorages(IPlayer player, NeoContext neoContext)
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

            await neoContext.AddAsync(playerStorage);
        }
    }
}