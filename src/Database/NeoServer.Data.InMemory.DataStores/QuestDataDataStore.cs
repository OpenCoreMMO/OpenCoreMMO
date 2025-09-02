using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Quest;

namespace NeoServer.Data.InMemory.DataStores;

public class QuestDataDataStore : DataStore<QuestDataDataStore, uint, Quest>,
    IQuestDataStore
{
}