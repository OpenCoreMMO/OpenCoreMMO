using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Server.Events.Items;

public class ItemStartedDecayingEventHandler
{
    private readonly IDecayableItemManager _decayableItemManager;

    public ItemStartedDecayingEventHandler(IDecayableItemManager decayableItemManager)
    {
        _decayableItemManager = decayableItemManager;
    }

    public void Execute(IItem item)
    {
        _decayableItemManager.Add(item);
    }
}