using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.DataStores;

namespace NeoServer.Domain.Quest;

public class QuestService(IQuestDataStore questDataStore)
{
    public Quest GetQuest(uint id)
    {
        return questDataStore.Get(id);
    }

    public bool QuestIsStarted(IPlayer player, uint questId)
    {
        var quest = GetQuest(questId);
        if (quest == null) return false;

        var storageValue = player.GetStorageValue(quest.Id);
        if (storageValue != -1 || storageValue >= (int)quest.StartValue) return true;

        return false;
    }

    public bool QuestIsCompleted(IPlayer player, uint questId)
    {
        var quest = GetQuest(questId);
        if (quest == null) return false;

        var missions = quest.Missions;
        if (missions == null) return true;

        foreach (var mission in missions)
            if (!MissionIsCompleted(player, mission))
                return false;

        return true;
    }

    public bool MissionIsCompleted(IPlayer player, Mission mission)
    {
        if (mission == null) return false;

        var value = player.GetStorageValue(mission.Id);

        if (value == -1) return false;

        if (mission.IgnoreEndValue) return value >= mission.EndValue;

        return value == mission.EndValue;
    }

    public bool MissionIsStarted(IPlayer player, Mission mission)
    {
        if (mission == null) return false;
        var value = player.GetStorageValue(mission.Id);

        return value != -1 && value >= mission.StartValue && (mission.IgnoreEndValue || value <= mission.EndValue);
    }

    public string GetMissionDescription(IPlayer player, Mission mission)
    {
        var playerMissionStateId = player.GetStorageValue(mission.Id);

        if (!string.IsNullOrEmpty(mission.Description))
        {
            var description = mission.Description;
            description = description.Replace("|STATE|", playerMissionStateId.ToString());
            description = description.Replace("\\n", "\n");
            return description;
        }

        return mission.GetStateDescription((uint)playerMissionStateId, mission.IgnoreEndValue) ?? mission.Description;
    }
}