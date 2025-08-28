using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Location.Structs;

namespace NeoServer.Domain.Items.Items.Containers;

public class Parcel(IItemType type, Location location, IEnumerable<IItem> children = null)
    : Container.Container(type, location, children)
{
    public int NumberOfLabels
    {
        get
        {
            if (!HasItems)
            {
                return 0;
            }

            Map.TryGetValue(GameConstants.LABEL_SERVER_ID, out var numberOfLabels);
            return (int)numberOfLabels;
        }
    }

    public Label Label
    {
        get
        {
            var (itemLabel, _, _) = GetFirstItemByServerId(GameConstants.LABEL_SERVER_ID);
            return itemLabel as Label;
        }
    }
}