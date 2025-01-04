using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using NeoServer.Game.Common.Contracts.Creatures;

namespace NeoServer.Loaders.Npcs;

public class NpcCustomAttributeLoader
{
    public static void LoadCustomData(INpcType type, NpcJsonData npcData)
    {
        if (type is null || npcData?.CustomData is null) return;
        List<CustomData> list = JsonSerializer.Deserialize<List<CustomData>>(npcData.CustomData);

        if (list == null || list.Count == 0) return;

        var map = new Dictionary<string, dynamic>();

        foreach (CustomData item in list)
            map.TryAdd(item.Key, item.Value);

        type.CustomAttributes.Add("custom-data", map);
    }
    
    public class CustomData
    {
        [JsonPropertyName("key")]
        public string Key { get; set; }
        
        [JsonPropertyName("value")]
        public dynamic Value { get; set; }
    }
}