using NeoServer.Domain.Common.Creatures;

namespace NeoServer.Domain.Common.Parsers;

public static class MessageParser
{
    public static string GetSkillAdvancedMessage(SkillType type, int fromLevel, int toLevel)
    {
        if (type == SkillType.Level)
            return $"You advanced from level {fromLevel} to level {toLevel}.";

        var skillText = type switch
        {
            SkillType.Sword => "sword fighting",
            SkillType.Club => "club fighting",
            SkillType.Axe => "axe fighting",
            SkillType.Distance => "distance fighting",
            SkillType.Magic => "magic",
            SkillType.Fishing => "fishing",
            SkillType.Shielding => "shielding",
            SkillType.Fist => "fist fighting",
            SkillType.Speed => "speed",
            SkillType.None => "skill",
            _ => "skill"
        };

        return $"You advanced to {skillText} level {toLevel}.";
    }

    public static string GetSkillRegressedMessage(SkillType type, int fromLevel, int toLevel)
    {
        if (type == SkillType.Level)
            return $"You were downgraded from level {fromLevel} to level {toLevel}.";

        var skillText = type switch
        {
            SkillType.Sword => "sword fighting",
            SkillType.Axe => "axe fighting",
            SkillType.Club => "club fighting",
            SkillType.Distance => "distance fighting",
            SkillType.Fishing => "fishing",
            SkillType.Fist => "fist fighting",
            SkillType.Magic => "magic level",
            SkillType.Shielding => "shield level",
            _ => "skill"
        };

        return $"Your {skillText} was downgraded from level {fromLevel} to level {toLevel}.";
    }
}