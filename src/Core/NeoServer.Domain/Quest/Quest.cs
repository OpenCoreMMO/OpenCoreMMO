namespace NeoServer.Domain.Quest;

public class Quest
{
    public uint Id => StartId;
    public string Name { get; set; }
    public uint StartId { get; set; }
    public uint StartValue { get; set; }
    public List<Mission> Missions { get; set; }

    public Mission GetMission(uint id)
    {
        return Missions.SingleOrDefault(m => m.Id == id);
    }
}

public class Mission
{
    public uint Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public uint StartValue { get; set; }
    public uint EndValue { get; set; }
    public bool IgnoreEndValue { get; set; }
    public List<MissionState> States { get; set; }

    public string GetStateDescription(uint stateId, bool ignoreEndValue)
    {
        var state = States?.FirstOrDefault(s => ignoreEndValue ? s.Id >= stateId : s.Id == stateId);
        return state?.Description;
    }
}

public class MissionState
{
    public uint Id { get; set; }
    public string Description { get; set; }
}