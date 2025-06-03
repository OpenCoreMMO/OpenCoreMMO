using NeoServer.Game.Common.Combat.Structs;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Item;

namespace NeoServer.Game.Combat.Attacks;

public static class FistCombatAttack
{
    public static bool Use(ICombatActor actor, ICombatActor enemy, out CombatAttackResult combatResult)
    {
        combatResult = new CombatAttackResult(DamageType.Melee);

        if (actor is not IPlayer player) return false;

        var maxDamage = player.CalculateAttackPower(0.085f, 7);
        var combat = new CombatAttackValue(actor.MinimumAttackPower,
            maxDamage, DamageType.Melee);

        if (!MeleeCombatAttack.CalculateAttack(actor, enemy, combat, out var damage)) return false;

        enemy.TakeDamage(actor, damage);

        return true;
    }
}