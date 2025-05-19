using NeoServer.Game.Combat.Services.Attacks;
using NeoServer.Game.Combat.Spells;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Combat.Structs;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.DataStores;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Common.Creatures;
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

    public override bool OnCast(ICombatActor caster, string words, out InvalidOperation error)
    {
        error = InvalidOperation.NotPossible;

        IThing target = caster.CurrentTarget;

        if (IsSelfTarget)
        {
            target = caster;
        }

        if (CasterNeedsTargetOrDirection && target is null)
        {
            var map = IoC.GetInstance<IMap>();
            target = map.GetNextTile(caster.Location, caster.Direction);
        }

        var attackInput = new AttackInput(caster,target, CombatSettings);
        CombatSettings.Range = Range;

        if (NeedDirection)
        {
            var effectStore = IoC.GetInstance<IAreaEffectStore>();
            var area = effectStore.Get(AreaName, caster.Direction);

            attackInput.Parameters.Area = area;
            attackInput.Parameters.NeedDirection = NeedDirection;
        }

        IoC.GetInstance<IAttackService>().Execute(attackInput);

        return true;
    }
}