using System.Linq;
using Microsoft.VisualBasic;
using NeoServer.Application.Common.Contracts.Scripts;
using NeoServer.Game.Common.Chats;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.DataStores;
using NeoServer.Networking.Packets.Incoming;
using NeoServer.Networking.Packets.Outgoing;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Commands;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Server.Commands.Player;

public class PlayerSayCommand : ICommand
{
    private readonly IChatChannelStore _chatChannelStore;
    private readonly IGameServer _game;
    private readonly ILuaManager _luaManager;

    public PlayerSayCommand(IGameServer game, IChatChannelStore chatChannelStore, ILuaManager luaManager)
    {
        _game = game;
        _chatChannelStore = chatChannelStore;
        _luaManager = luaManager;
    }

    public void Execute(IPlayer player, IConnection connection, PlayerSayPacket playerSayPacket)
    {
        if (string.IsNullOrWhiteSpace(playerSayPacket.Message) ||
            (playerSayPacket.Message?.Length ?? 0) > 255) return;
        if ((playerSayPacket.Receiver?.Length ?? 0) > 30) return;

        var message = playerSayPacket.Message?.Trim();

        //estava chamando aqui lua manager
        var messageData = message.Split(" ");

        var talkAction = _luaManager.GetTalkAction(messageData[0]);
        var parameter = "";

        if (messageData.Count() > 1)
            parameter = messageData[1];

        if (talkAction != null)
        {
            talkAction.ExecuteSay(player, messageData[0], parameter, playerSayPacket.TalkType);
        }

        if (player.CastSpell(message)) return;

        switch (playerSayPacket.TalkType)
        {
            case SpeechType.None:
                break;
            case SpeechType.Say:
                player.Say(playerSayPacket.Message, playerSayPacket.TalkType);
                break;
            case SpeechType.Whisper:
                break;
            case SpeechType.Yell:
                break;
            case SpeechType.PrivatePlayerToNpc:
                SendMessageToNpc(player, playerSayPacket, message);
                break;
            case SpeechType.PrivateNpcToPlayer:
                break;

            case SpeechType.ChannelOrangeText:
            case SpeechType.ChannelRed1Text:
            case SpeechType.ChannelYellowText:
                SendMessageToChannel(player, playerSayPacket.ChannelId, message);
                break;

            case SpeechType.ChannelRed2Text:
                break;
            case SpeechType.ChannelWhiteText:
                break;
            case SpeechType.RvrChannel:
                break;
            case SpeechType.RvrAnswer:
                break;
            case SpeechType.RvrContinue:
                break;
            case SpeechType.Broadcast:
                break;
            case SpeechType.Private:
            case SpeechType.PrivateRed:
                SendMessageToPlayer(player, connection, playerSayPacket, message);
                break;
            case SpeechType.MonsterSay:
                break;
            case SpeechType.MonsterYell:
                break;
        }
    }

    private void SendMessageToPlayer(IPlayer player, IConnection connection, PlayerSayPacket playerSayPacket,
        string message)
    {
        if (string.IsNullOrWhiteSpace(playerSayPacket.Receiver) ||
            !_game.CreatureManager.TryGetPlayer(playerSayPacket.Receiver, out var receiver))
        {
            connection.OutgoingPackets.Enqueue(new TextMessagePacket("A player with this name is not online.",
                TextMessageOutgoingType.Small));
            connection.Send();
            return;
        }

        player.SendMessageTo(receiver, playerSayPacket.TalkType, message);
    }

    private void SendMessageToNpc(IPlayer player, PlayerSayPacket playerSayPacket, string message)
    {
        foreach (var creature in _game.Map.GetCreaturesAtPositionZone(player.Location))
            if (creature is INpc npc)
            {
                npc.Hear(player, playerSayPacket.TalkType, message);
                return;
            }
    }

    private void SendMessageToChannel(IPlayer player, ushort channelId, string message)
    {
        var channel = _chatChannelStore.Get(channelId);

        if (channel is null) channel = player.Channels.PrivateChannels.FirstOrDefault(x => x.Id == channelId);

        if (channel is null) return;

        player.Channels.SendMessage(channel, message);
    }
}