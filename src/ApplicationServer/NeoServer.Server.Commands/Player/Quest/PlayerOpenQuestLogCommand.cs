using System.Collections.Generic;
using System.Linq;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Quest;
using NeoServer.Networking.Packets.Outgoing.Player;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Commands;

namespace NeoServer.Server.Commands.Player.Quest;

public class PlayerOpenQuestLogCommand(IQuestDataStore questDataStore, QuestService questService, IGameCreatureManager gameCreatureManager)
    : ICommand
{
    public void Execute(IPlayer player)
    {
        var quests = questDataStore.All.ToList();

        if (!gameCreatureManager.GetPlayerConnection(player.CreatureId, out var connection)) return;

        var playerQuests = new List<PlayerQuestLogPacket.Quest>();

        foreach (var quest in quests)
        {
            if (!questService.QuestIsStarted(player, quest.Id)) continue;
            
            var isCompleted = questService.QuestIsCompleted(player, quest.Id);
            playerQuests.Add(new PlayerQuestLogPacket.Quest((ushort)quest.Id, quest.Name, true, isCompleted));
        }

        connection.OutgoingPackets.Enqueue(new PlayerQuestLogPacket()
        {
            NumberOfQuests = (ushort)playerQuests.Count,
            Quests = playerQuests
        });

        connection.Send();
    }
}