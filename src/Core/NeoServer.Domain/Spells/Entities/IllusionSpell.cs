using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Results;
using NeoServer.Domain.Creatures;
using NeoServer.Domain.Creatures.Conditions.Enums;
using NeoServer.Domain.Spells.Entities;

namespace NeoServer.Domain.Spells;

public class IllusionSpell(uint duration, string creatureName, IMonsterTypeStore monsterTypeStore, EffectT effect) : Spell<IllusionSpell>
{
    public override string Name => "Illusion";
    public override EffectT Effect { get; } = effect;
    public override uint Duration { get; } = duration;
    public override ushort ManaConsumption => 100;
    public override ConditionType ConditionType => ConditionType.Illusion;
    public virtual IMonsterTypeStore MonsterTypeStore { get; } = monsterTypeStore;
    public virtual string CreatureName { get; } = creatureName;

    public override Result OnCast(ICombatActor caster, IThing target, bool isHotkey)
    {
        if (!MonsterTypeStore.TryGetValue(CreatureName, out var monster)) return Result.NotApplicable;

        var look = monster.Look;

        look.TryGetValue(LookType.Type, out var lookType);
        look.TryGetValue(LookType.Addon, out var addon);
        look.TryGetValue(LookType.Body, out var body);
        look.TryGetValue(LookType.Feet, out var feet);
        look.TryGetValue(LookType.Legs, out var legs);
        look.TryGetValue(LookType.Head, out var head);

        caster.SetTemporaryOutfit(lookType, (byte)head, (byte)body, (byte)legs, (byte)feet, (byte)addon);

        return Result.Success;
    }

    public override void OnEnd(ICombatActor actor)
    {
        actor.BackToOldOutfit();
        base.OnEnd(actor);
    }
}