using NeoServer.Game.Combat;
using NeoServer.Game.Combat.Attacks;
using NeoServer.Game.Common.Combat;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Results;
using NeoServer.Modules.Combat.Attacks.InAreaAttack;
using NeoServer.Modules.Combat.Defense;
using NeoServer.Modules.Combat.Defense.MonsterDefense;
using NeoServer.Modules.Combat.Defense.PlayerDefense;

namespace NeoServer.Modules.Combat.Attacks.DistanceAttack;

public sealed class DistanceAttackStrategy(
    DistanceAttackValidation distanceAttackValidation,
    AttackCalculation attackCalculation,
    AreaAttackProcessor areaAttackProcessor,
    DefenseHandler defenseHandler)
    : AttackStrategy
{
    public override string Name => nameof(DistanceAttackStrategy);

    protected override Result Attack(in AttackInput attackInput)
    {
        var aggressor = attackInput.Aggressor;
        var target = attackInput.Target;
        var creatureAggressor = attackInput.Aggressor as ICombatActor;

        if (attackInput.Parameters.NeedTarget && target is not ICombatActor)
            return Result.NotApplicable;

        var validationResult = distanceAttackValidation.Validate(attackInput);
        if (validationResult.Failed) return validationResult;

        var missAttackResult = CalculateIfMissedAttack(creatureAggressor, target, attackInput.Parameters);

        creatureAggressor?.PreAttack(new PreAttackValues
        {
            Aggressor = creatureAggressor,
            Target = target,
            MissLocation = missAttackResult.Destination,
            ShootType = attackInput.Parameters.ShootType
        });

        var result = true;

        if (!missAttackResult.Missed)
        {
            result = ApplyDamage(attackInput, target);
        }

        creatureAggressor?.PostAttack(attackInput);
        return result ? Result.Success : Result.NotApplicable;
    }

    private bool ApplyDamage(AttackInput attackInput, IThing victim)
    {
        var primaryDamage = attackCalculation.Calculate(attackInput.Parameters.MinDamage,
            attackInput.Parameters.MaxDamage,
            attackInput.Parameters.DamageType);

        var extraAttack = attackInput.Parameters.ExtraAttack;

        //initialize damage list
        Span<CombatDamage> damages = stackalloc CombatDamage[attackInput.Parameters.HasExtraAttack ? 2 : 1];

        damages[0] = primaryDamage;

        if (attackInput.Parameters.HasExtraAttack) AddElementalAttacks(extraAttack, damages);

        if (attackInput.Parameters.IsAttackInArea)
        {
            areaAttackProcessor.Propagate(attackInput, new CombatDamageList(damages));
            return true;
        }

        defenseHandler.Handle(attackInput.Aggressor, victim as ICombatActor, new CombatDamageList(damages));
     
        return true;
    }

    private static MissAttackResult CalculateIfMissedAttack(ICombatActor aggressor, IThing victim,
        AttackParameter parameter)
    {
        if (aggressor is null || victim is null) return MissAttackResult.NotMissed;

        if (parameter.IsMagicalAttack) return MissAttackResult.NotMissed;

        var player = aggressor as IPlayer;

        var missAttackValues = new MissAttackCalculationValues
        (
            aggressor.Location,
            victim.Location,
            player?.Inventory?.Weapon,
            player?.GetSkillLevel(player.SkillInUse) ?? default
        );

        var missAttackResult = MissAttackCalculation.Calculate(missAttackValues);
        return missAttackResult;
    }

    private void AddElementalAttacks(ExtraAttack extraAttack, Span<CombatDamage> damages)
    {
        damages[1] = attackCalculation.Calculate(extraAttack.MinDamage,
            extraAttack.MaxDamage, extraAttack.DamageType);
    }
}