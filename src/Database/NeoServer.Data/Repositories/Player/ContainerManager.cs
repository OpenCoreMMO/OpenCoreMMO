using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServer.Data.Contexts;
using NeoServer.Data.Entities;
using NeoServer.Data.Parsers;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Common.Helpers;

namespace NeoServer.Data.Repositories.Player;

public static class ContainerManager
{
    public static async Task Save<TPlayerItemEntity>(IPlayer player, IContainer container, NeoContext neoContext)
        where TPlayerItemEntity : PlayerItemBaseEntity, new()
    {
        await Save<TPlayerItemEntity>((int)player.Id, container, neoContext);
    }

    public static async Task Save<TPlayerItemEntity>(int playerId, IContainer container, NeoContext neoContext,
        bool includeContainer = false)
        where TPlayerItemEntity : PlayerItemBaseEntity, new()
    {
        if (playerId == 0) return;
        if (Guard.AnyNull(container)) return;

        if (container?.Items?.Count == 0 && !includeContainer) return;

        var containerId = 0;
        var containers = new Queue<(IContainer Container, int ParentId)>();

        // Save the container itself if includeContainer is true
        if (includeContainer)
        {
            var containerEntity = ItemEntityParser.ToPlayerItemEntity<TPlayerItemEntity>(container);
            if (containerEntity != null)
            {
                containerEntity.PlayerId = playerId;
                containerEntity.ParentId = 0;
                containerEntity.ContainerId = ++containerId;
                await neoContext.AddAsync(containerEntity);
            }
        }

        containers.Enqueue((container, includeContainer ? containerId : 0));

        while (containers.TryDequeue(out var dequeuedContainer))
        {
            var items = dequeuedContainer.Container.Items;
            if (items.Count == 0) continue;

            foreach (var item in items)
            {
                var itemModel = ItemEntityParser.ToPlayerItemEntity<TPlayerItemEntity>(item);
                if (itemModel is null) continue;

                itemModel.PlayerId = playerId;
                itemModel.ParentId = dequeuedContainer.ParentId;

                if (item is IContainer innerContainer)
                {
                    itemModel.ContainerId = ++containerId;
                    containers.Enqueue((innerContainer, itemModel.ContainerId));
                }

                await neoContext.AddAsync(itemModel);
            }
        }
    }
}