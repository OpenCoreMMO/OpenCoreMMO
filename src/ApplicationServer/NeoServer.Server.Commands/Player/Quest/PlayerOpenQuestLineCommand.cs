using System.Linq;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Quest;
using NeoServer.Networking.Packets.Outgoing.Player;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Commands;

namespace NeoServer.Server.Commands.Player.Quest;

public class PlayerOpenQuestLineCommand(
    QuestService questService,
    IGameCreatureManager gameCreatureManager)
    : ICommand
{
    public void Execute(IPlayer player, uint questId)
    {
        var quest = questService.GetQuest(questId);

        if (!gameCreatureManager.GetPlayerConnection(player.CreatureId, out var connection)) return;
        
        var missions = quest.Missions
            ?.Where(m => questService.MissionIsStarted(player, m))
            .Select(m => new PlayerQuestLinePacket.Mission(m.Name, questService.GetMissionDescription(player, m), true))
            .ToList();

        connection.OutgoingPackets.Enqueue(new PlayerQuestLinePacket
        {
            QuestId = (ushort)quest.Id,
            Missions = missions,
            NumberOfMissions = (byte?)missions?.Count ?? 0
        });

        connection.Send();
    }
}