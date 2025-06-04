using NeoServer.Domain.Chat;
using NeoServer.Domain.Common.Contracts.DataStores;

namespace NeoServer.Data.InMemory.DataStores;

public class ChatChannelStore : DataStore<ChatChannelStore, ushort, ChatChannel>, IChatChannelStore
{
}