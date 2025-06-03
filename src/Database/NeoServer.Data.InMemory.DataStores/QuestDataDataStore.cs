using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Common.Item;

namespace NeoServer.Data.InMemory.DataStores;

public class QuestDataDataStore : DataStore<QuestDataDataStore, (ushort ActionId, uint UniqueId), QuestData>,
    IQuestDataStore
{
}