using NeoServer.Domain.Common.Contracts.Items.Types;

namespace NeoServer.Domain.Items.Items.Containers.Container.Operations;

internal static class ContainerClearOperation
{
    public static void Clear(Container container)
    {
        container.SetParent(null);

        if (container.Items is null) return;

        while (container.Items.FirstOrDefault() is { } item)
        {
            switch (item)
            {
                case IContainer innerContainer:
                    container.DetachEvents(innerContainer);
                    innerContainer.Clear();
                    break;
                case ICumulative cumulative:
                    cumulative.OnReduced -= container.OnItemReduced;
                    break;
            }

            container.RemoveItem(item, item.Amount);
        }

        container.Items.Clear();
        container.SlotsUsed = 0;
    }
}