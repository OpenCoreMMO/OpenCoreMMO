using NeoServer.Domain.Combat.Services.Attacks;
using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Results;
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

        var attackInput = new AttackInput(caster, target, CombatSettings);
        CombatSettings.Range = Range;

        if (NeedDirection)
        {
            var effectStore = IoC.GetInstance<IAreaEffectStore>();
            var area = effectStore.Get(AreaName, caster.Direction);

            attackInput.Parameters.Area = area;
            attackInput.Parameters.NeedDirection = NeedDirection;
        }

        IoC.GetInstance<IAttackService>().Execute(attackInput);

        return Result.Success;
    }
}