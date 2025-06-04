using NeoServer.Domain.Chat;

namespace NeoServer.Domain.Common.Contracts.DataStores;

public interface IChatChannelStore : IDataStore<ushort, ChatChannel>
{
}