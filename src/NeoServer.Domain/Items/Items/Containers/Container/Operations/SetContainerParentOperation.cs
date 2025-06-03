using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Creatures.Players;
using NeoServer.Domain.Common.Location.Structs;

namespace NeoServer.Domain.Items.Items.Containers.Container.Operations;

internal static class SetContainerParentOperation
{
    public static void SetParent(Container container, IThing thing)
    {
        container.Parent = thing;
        if (container.Parent is IPlayer) container.Location = new Location(Slot.Backpack);
        container.SetOwner(container.RootParent);
    }
}