using System.Linq;
using NeoServer.Domain.Chat;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Spells;
using NeoServer.Networking.Packets.Incoming;
using NeoServer.Networking.Packets.Outgoing;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Commands;
using NeoServer.Server.Common.Contracts.Network;
using NeoServer.Server.Common.Contracts.Scripts;

namespace NeoServer.Server.Commands.Player;

public class PlayerSayCommand(
    IGameServer game,
    IChatChannelStore chatChannelStore,
    IScriptManager scriptManager,
    SpellService spellService,
    SpellListManager spellListManager)
    : ICommand
{
    public async void Execute(IPlayer player, IConnection connection, PlayerSayPacket playerSayPacket)
    {
        if (string.IsNullOrWhiteSpace(playerSayPacket.Message) ||
            (playerSayPacket.Message?.Length ?? 0) > 255) return;
        if ((playerSayPacket.Receiver?.Length ?? 0) > 30) return;

        var message = playerSayPacket.Message?.Trim();

        if (scriptManager.TalkActions.Say(player, playerSayPacket.TalkType, message))
            return;

        //cast spell;
        if (spellListManager.TryGetInstantSpell(message?.Trim(), out var spell))
        {
            spellService.Cast(player, null, spell, false);
            return;
        }

        //if (player.CastSpell(message)) return;

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

            case SpeechType.ChannelOrange:
            case SpeechType.ChannelRed1:
            case SpeechType.ChannelYellow:
                SendMessageToChannel(player, playerSayPacket.ChannelId, message);
                break;

            //case SpeechType.ChannelRed2:
            //    break;
            //case SpeechType.ChannelWhite:
            //    break;
            //case SpeechType.RvrChannel:
            //    break;
            //case SpeechType.RvrAnswer:
            //    break;
            //case SpeechType.RvrContinue:
            //    break;
            case SpeechType.Broadcast:
                break;
            case SpeechType.PrivateFrom:
            case SpeechType.PrivateRedFrom:
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
            !game.CreatureManager.TryGetPlayer(playerSayPacket.Receiver, out var receiver))
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
        foreach (var creature in game.Map.GetCreaturesAtPositionZone(player.Location))
            if (creature is INpc npc)
            {
                npc.Hear(player, playerSayPacket.TalkType, message);
                return;
            }
    }

    private void SendMessageToChannel(IPlayer player, ushort channelId, string message)
    {
        var channel = chatChannelStore.Get(channelId);

        if (channel is null) channel = player.Channels.PrivateChannels.FirstOrDefault(x => x.Id == channelId);

        if (channel is null) return;

        player.Channels.SendMessage(channel, message);
    }
}