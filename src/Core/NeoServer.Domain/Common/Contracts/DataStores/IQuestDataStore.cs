using NeoServer.Domain.Common.Item;

namespace NeoServer.Domain.Common.Contracts.DataStores;

public interface IQuestDataStore : IDataStore<uint, Quest.Quest>
{
}