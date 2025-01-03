using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using NeoServer.Game.Common.Creatures;

namespace NeoServer.Loaders.Converts;

public class SkillConverter : JsonConverter<Dictionary<SkillType, float>>
{
    public override Dictionary<SkillType, float> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException("Expected StartArray token.");
        }

        var list = new List<Dictionary<string, string>>();

        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            var item = JsonSerializer.Deserialize<Dictionary<string, string>>(ref reader, options);
            if (item != null)
            {
                list.Add(item);
            }
        }

        return list.ToDictionary(
            x => ParseSkillName(x["name"]),
            x => float.Parse(x["multiplier"], CultureInfo.InvariantCulture.NumberFormat));
    }

    private static SkillType ParseSkillName(string skillName)
    {
        return skillName switch
        {
            "fist" => SkillType.Fist,
            "axe" => SkillType.Axe,
            "sword" => SkillType.Sword,
            "club" => SkillType.Club,
            "shielding" => SkillType.Shielding,
            "fishing" => SkillType.Fishing,
            "distance" => SkillType.Distance,
            "magic" => SkillType.Magic,
            "level" => SkillType.Level,
            "speed" => SkillType.Speed,
            _ => throw new ArgumentOutOfRangeException(nameof(skillName), skillName, null)
        };
    }

    public override void Write(Utf8JsonWriter writer, Dictionary<SkillType, float> value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}