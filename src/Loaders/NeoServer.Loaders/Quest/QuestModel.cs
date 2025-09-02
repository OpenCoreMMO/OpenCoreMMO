using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NeoServer.Loaders.Quest;

public class QuestModel
{
    [JsonPropertyName("name")] public string Name { get; set; }
    [JsonPropertyName("startstorageid")] public uint StartId { get; set; }
    [JsonPropertyName("startstoragevalue")] public uint StartValue { get; set; }
    [JsonPropertyName("missions")] public List<MissionModel> Missions { get; set; }
}

public class MissionModel
{
    [JsonPropertyName("storageid")] public uint Id { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; }
    [JsonPropertyName("startvalue")] public uint StartValue { get; set; }
    [JsonPropertyName("endvalue")] public uint EndValue { get; set; }
    [JsonPropertyName("ignoreendvalue")] public bool IgnoreEndValue { get; set; }
    [JsonPropertyName("states")] public List<MissionStateModel> States { get; set; }
    [JsonPropertyName("description")]public string Description { get; set; }
}

public class MissionStateModel
{
    [JsonPropertyName("id")] public uint Id { get; set; }
    [JsonPropertyName("description")] public string Description { get; set; }
}