using NeoServer.Game.Combat;
using NeoServer.Game.Combat.Attacks;
using NeoServer.Game.Common.Combat;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Results;
using NeoServer.Modules.Combat.Attacks.InAreaAttack;
using NeoServer.Modules.Combat.Defense;
using NeoServer.Modules.Combat.Defense.MonsterDefense;
using NeoServer.Modules.Combat.Defense.PlayerDefense;

namespace NeoServer.Modules.Combat.Attacks.MeleeAttack;

public sealed class MeleeAttackStrategy(
    MeleeAttackValidation meleeAttackValidation,
    AttackCalculation attackCalculation,
    AreaAttackProcessor areaAttackProcessor,
    DefenseHandler defenseHandler)
    : AttackStrategy
{
    public override string Name { get; } = nameof(MeleeAttackStrategy);

    /// <summary>
    /// Executes the melee attack, validating the attack and applying damage to the target.
    /// </summary>
    protected override Result Attack(in AttackInput attackInput)
    {
        var aggressor = attackInput.Aggressor as ICombatActor;

        // Ensure the target is a valid combat actor (e.g., player or monster)
        if (attackInput.Target is not ICombatActor victim) return Result.NotApplicable;

        // Validate if the melee attack can be performed based on aggressor and target
        var validationResult = meleeAttackValidation.Validate(aggressor, victim);
        if (validationResult.Failed) return validationResult;

        aggressor?.PreAttack(new PreAttackValues
        {
            Aggressor = aggressor,
            Target = victim
        });

        // Calculate and apply damage
        ApplyDamage(attackInput);

        aggressor?.PostAttack(attackInput);

        return Result.Success;
    }

    /// <summary>
    /// Calculates and applies damage to the target, handling area attacks and defense mechanics.
    /// </summary>
    private void ApplyDamage(in AttackInput attackInput)
    {
        // Allocate space for one or two damages based on whether there is an extra attack
        Span<CombatDamage> damages = stackalloc CombatDamage[attackInput.Parameters.HasExtraAttack ? 2 : 1];

        var extraAttack = attackInput.Parameters.ExtraAttack;

        var physicalDamage = attackCalculation.Calculate(attackInput.Parameters.MinDamage,
            attackInput.Parameters.MaxDamage,
            attackInput.Parameters.DamageType);

        damages[0] = physicalDamage;

        // If there's an extra elemental attack, calculate and add it to the buffer
        if (attackInput.Parameters.HasExtraAttack)
        {
            //Adds an elemental attack to the damage buffer, using the extra attack parameters.
            damages[1] = attackCalculation.Calculate(extraAttack.MinDamage,
                extraAttack.MaxDamage, extraAttack.DamageType);
        }

        var combatDamageList = new CombatDamageList(damages);

        // Handle area attacks separately by propagating damage to all affected targets
        if (attackInput.Parameters.IsAttackInArea)
        {
            areaAttackProcessor.Propagate(attackInput, combatDamageList);
            return;
        }

        // Handle defense logic based on the type of target (player or monster)
        defenseHandler.Handle(attackInput.Aggressor,  attackInput.Target as ICombatActor, combatDamageList);
    }
}