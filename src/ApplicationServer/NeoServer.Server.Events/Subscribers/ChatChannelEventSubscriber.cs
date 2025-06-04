using System;
using NeoServer.Domain.Chat;
using NeoServer.Domain.Common.Contracts.Chats;
using NeoServer.Server.Events.Chat;

namespace NeoServer.Server.Events.Subscribers;

public class ChatChannelEventSubscriber : IChatChannelEventSubscriber
{
    private readonly ChatMessageAddedEventHandler chatMessageAddedEventHandler;

    public ChatChannelEventSubscriber(ChatMessageAddedEventHandler chatMessageAddedEventHandler)
    {
        this.chatMessageAddedEventHandler = chatMessageAddedEventHandler;
    }

    public void Subscribe(ChatChannel chatChannel)
    {
        chatChannel.OnMessageAdded += chatMessageAddedEventHandler.Execute;
    }

    public void Unsubscribe(ChatChannel chatChannel)
    {
        throw new NotImplementedException();
    }
}