using NeoServer.Domain.Combat.Attacks;
using NeoServer.Domain.Combat.Services.Attacks;
using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Results;
using NeoServer.Domain.Creatures.Condition;
using NeoServer.Domain.Spells;
using NeoServer.Server.Helpers;

namespace NeoServer.Extensions.Spells.Attack;

public abstract class AttackSpell : Spell<AttackSpell>
{
    public override EffectT Effect => EffectT.None;
    public override ConditionType ConditionType => ConditionType.None;
    public override uint Duration { get; }
    protected abstract CombatParameter CombatSettings { get; }
    protected virtual string AreaName { get; }
    protected virtual bool IsSelfTarget { get; }

    public override Result OnCast(ICombatActor caster, IThing target, bool isHotkey)
    {
        if (IsSelfTarget) target = caster;

        if (CasterNeedsTargetOrDirection && target is null)
        {
            var map = IoC.GetInstance<IMap>();
            target = map.GetNextTile(caster.Location, caster.Direction);
        }

        var combatParameter = new CombatParameter
        {
            Effect = CombatSettings.Effect,
            DamageType = CombatSettings.DamageType,
            ShootType = CombatSettings.ShootType,
            DamageFormula = CombatSettings.DamageFormula,
            Area = CombatSettings.Area,
            NeedDirection = CombatSettings.NeedDirection,
            FieldAttack = CombatSettings.FieldAttack,
            BlockArmor = CombatSettings.BlockArmor,
            Condition = CombatSettings.Condition,
            Range = Range,
            MinDamage = CombatSettings.MinDamage,
            MaxDamage = CombatSettings.MaxDamage,
            Radius = CombatSettings.Radius,
            Length = CombatSettings.Length,
            Spread = CombatSettings.Spread,
            ExtraAttack = CombatSettings.ExtraAttack,
            CooldownType = CombatSettings.CooldownType,
            CooldownId = CombatSettings.CooldownId,
            IsMagicalAttack = CombatSettings.IsMagicalAttack,
            CreateItemId = CombatSettings.CreateItemId,
            CooldownDuration = CombatSettings.CooldownDuration,
            HitChance = CombatSettings.HitChance,
            CoordinateArea = CombatSettings.CoordinateArea,
            UsingWeapon = CombatSettings.UsingWeapon
        };

        var attackInput = new AttackInput(caster, target, combatParameter);

        if (caster is IMonster monster)
            if (monster.Metadata.Spells.TryGetValue(Name, out var attack))
            {
                combatParameter.MinDamage = attack.CombatParameter.MinDamage;
                combatParameter.MaxDamage = attack.CombatParameter.MaxDamage;
                combatParameter.CooldownId = attack.Id;
            }

        if (NeedDirection)
        {
            var effectStore = IoC.GetInstance<IAreaEffectStore>();
            var area = effectStore.Get(AreaName, caster.Direction);

            attackInput.Parameters.Area = area;
            attackInput.Parameters.NeedDirection = NeedDirection;
        }

        return IoC.GetInstance<IAttackService>().Execute(attackInput);
    }
}