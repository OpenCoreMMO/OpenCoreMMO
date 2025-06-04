using NeoServer.Domain.Common.Creatures;

namespace NeoServer.Loaders.Monsters.Converters;

public class MonsterAttributeParser
{
    public static EffectT ParseAreaEffect(string type)
    {
        return type switch
        {
            "blueshimmer" => EffectT.GlitterBlue,
            "redshimmer" => EffectT.GlitterRed,
            "greenshimmer" => EffectT.GlitterGreen,
            "mortarea" => EffectT.BubbleBlack,
            "groundshaker" => EffectT.GroundShaker,
            _ => EffectT.None
        };
    }
}