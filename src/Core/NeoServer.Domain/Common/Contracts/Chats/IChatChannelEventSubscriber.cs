using NeoServer.Domain.Chat;

namespace NeoServer.Domain.Common.Contracts.Chats;

public interface IChatChannelEventSubscriber
{
    public void Subscribe(ChatChannel chatChannel);
    public void Unsubscribe(ChatChannel chatChannel);
}