using NeoServer.Domain.Common.Creatures;

namespace NeoServer.Domain.Common.Parsers;

public static class SkillTypeParser
{
    public static string Parse(SkillType type)
    {
        return type switch
        {
            SkillType.Axe => "Axe Fighting",
            SkillType.Club => "Club Fighting",
            SkillType.Distance => "Distance Fighting",
            SkillType.Fishing => "Fishing",
            SkillType.Fist => "Fist Fighting",
            SkillType.Magic => "Magic Level",
            SkillType.Shielding => "Shielding",
            SkillType.Speed => "Speed",
            SkillType.Sword => "Sword Fighting",
            _ => string.Empty
        };
    }
}