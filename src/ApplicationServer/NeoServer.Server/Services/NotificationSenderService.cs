using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Networking.Packets.Outgoing;

namespace NeoServer.Server.Services;

public static class NotificationSenderService
{
    public static event SendNotification OnNotificationSent;

    public static void Send(IPlayer to, string message,
        TextMessageOutgoingType notificationType = TextMessageOutgoingType.Description)
    {
        OnNotificationSent?.Invoke(to, message, notificationType);
    }
}

public delegate void SendNotification(IPlayer player, string message, TextMessageOutgoingType notificationType);