using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types.Containers;

namespace NeoServer.Domain.Items.Items.Containers.Container.Queries;

internal static class FindRootParentQuery
{
    public static IThing Find(IContainer container)
    {
        IThing root = container;
        while (root is IContainer { Parent: not null } parentContainer) root = parentContainer.Parent;
        return root;
    }
}