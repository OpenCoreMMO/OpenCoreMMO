using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types.Containers;

namespace NeoServer.Domain.Items.Items.Containers.Container.Queries;

internal static class GetRecursiveItemsQuery
{
    public static List<IItem> Get(IContainer container)
    {
        var items = new List<IItem>();

        foreach (var item in container.Items)
        {
            if (item is not IContainer innerContainer)
            {
                items.Add(item);
                continue;
            }

            items.Add(innerContainer);
            items.AddRange(Get(innerContainer));
        }

        return items;
    }
}