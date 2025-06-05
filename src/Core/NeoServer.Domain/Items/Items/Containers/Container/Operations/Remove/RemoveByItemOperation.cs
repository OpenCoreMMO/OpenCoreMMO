using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types;

namespace NeoServer.Domain.Items.Items.Containers.Container.Operations.Remove;

internal static class RemoveByItemOperation
{
    public static void Remove(Container fromContainer, IItem item, byte amount) //todo: slow method
    {
        var containers = new Queue<IContainer>();
        containers.Enqueue(fromContainer);

        while (containers.TryDequeue(out var container))
        {
            byte slotIndex = 0;
            foreach (var containerItem in container.Items)
            {
                if (containerItem is IContainer innerContainer) containers.Enqueue(innerContainer);
                if (containerItem != item)
                {
                    slotIndex++;
                    continue;
                }

                RemoveBySlotIndexOperation.Remove(fromContainer, slotIndex, amount);
                return;
            }
        }
    }
}