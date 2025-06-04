using NeoServer.Domain.Common.Contracts.Items;

namespace NeoServer.Server.Common.Contracts;

public interface IDecayableItemManager
{
    void Add(IItem item);
    void DecayExpiredItems();
}