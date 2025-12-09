using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Helpers;

namespace NeoServer.Domain.Creatures.Services;

public class HealService
{
    public void Heal(ICreature actor, ICombatActor target, HealType healType, ushort min, ushort max)
    {
        if (target is null) return;
        
        var value = (ushort)GameRandom.Random.Next(minValue: min, maxValue: max);

        if (healType is HealType.Health)
        {
            target.Heal(value, actor);
            return;
        }

        if (target is IPlayer targetPlayer)
        {
            switch (healType)
            {
                case HealType.Mana:
                    targetPlayer.IncreaseMana(value);
                    break;
                case HealType.Soul:
                    targetPlayer.HealSoul(value);
                    break;
            }
        }
    }
}

public enum HealType
{
    Health,
    Mana,
    Soul
}