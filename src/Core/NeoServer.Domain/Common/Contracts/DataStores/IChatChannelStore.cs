using NeoServer.Domain.Common.Contracts.Chats;

namespace NeoServer.Domain.Common.Contracts.DataStores;

public interface IChatChannelStore : IDataStore<ushort, IChatChannel>
{
}