using System;
using NeoServer.Game.Combat.Spells;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Results;

namespace NeoServer.Extensions.Spells.Support.Knight;

public class BloodRage : Spell<Food>
{
    public override uint Duration => 10_000;
    public override ConditionType ConditionType => ConditionType.Strengthened;
    public override EffectT Effect => EffectT.GlitterBlue;
    public override Result OnCast(ICombatActor caster, IThing target, bool isHotkey)
    {
        if (caster is not IPlayer player) return Result.NotApplicable;

        player.AddSkillBonus(SkillType.Axe, (sbyte)Math.Abs(player.Skills[SkillType.Axe].Level * 0.35));
        player.AddSkillBonus(SkillType.Sword, (sbyte)Math.Abs(player.Skills[SkillType.Axe].Level * 0.35));
        player.AddSkillBonus(SkillType.Club, (sbyte)Math.Abs(player.Skills[SkillType.Axe].Level * 0.35));
        player.AddSkillBonus(SkillType.Fist, (sbyte)Math.Abs(player.Skills[SkillType.Axe].Level * 0.35));

        caster.DisableShieldDefense();
        caster.IncreaseDamageReceived(15);
        return Result.Success;
    }

    public override void OnEnd(ICombatActor actor)
    {
        if (actor is not IPlayer player) return;

        player.RemoveSkillBonus(SkillType.Axe, (sbyte)Math.Abs(player.Skills[SkillType.Axe].Level * 0.35));
        player.RemoveSkillBonus(SkillType.Sword, (sbyte)Math.Abs(player.Skills[SkillType.Axe].Level * 0.35));
        player.RemoveSkillBonus(SkillType.Club, (sbyte)Math.Abs(player.Skills[SkillType.Axe].Level * 0.35));
        player.RemoveSkillBonus(SkillType.Fist, (sbyte)Math.Abs(player.Skills[SkillType.Axe].Level * 0.35));

        actor.EnableShieldDefense();
        actor.DecreaseDamageReceived(15);

        base.OnEnd(actor);
    }
}