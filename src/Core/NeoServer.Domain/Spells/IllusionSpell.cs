using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Results;

namespace NeoServer.Domain.Spells;

public class IllusionSpell : Spell<IllusionSpell>
{
    public IllusionSpell(uint duration, string creatureName, IMonsterDataManager monsters, EffectT effect)
    {
        Monsters = monsters;
        Duration = duration;
        Effect = effect;
        CreatureName = creatureName;
    }

    public override string Name => "Illusion";
    public override EffectT Effect { get; } = EffectT.GlitterGreen;
    public override uint Duration { get; } = 5000;
    public override ushort ManaConsumption => 100;
    public override ConditionType ConditionType => ConditionType.Illusion;
    public virtual IMonsterDataManager Monsters { get; }
    public virtual string CreatureName { get; }

    public override Result OnCast(ICombatActor caster, IThing target, bool isHotkey)
    {
        if (!Monsters.TryGetMonster(CreatureName, out var monster)) return Result.NotApplicable;

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