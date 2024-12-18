using System;
using NeoServer.Game.Combat.Spells;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Creatures;

namespace NeoServer.Extensions.Spells.Support.Knight;

public class BloodRage : Spell<Food>
{
    public override uint Duration => 10_000;
    public override ConditionType ConditionType => ConditionType.Strengthened;
    public override EffectT Effect => EffectT.GlitterBlue;

    public override bool OnCast(ICombatActor actor, string words, out InvalidOperation error)
    {
        error = InvalidOperation.None;

        if (actor is not IPlayer player) return false;
        
        player.AddSkillBonus(SkillType.Axe, (sbyte)Math.Abs(player.Skills[SkillType.Axe].Level * 0.35));
        player.AddSkillBonus(SkillType.Sword, (sbyte)Math.Abs(player.Skills[SkillType.Axe].Level * 0.35));
        player.AddSkillBonus(SkillType.Club, (sbyte)Math.Abs(player.Skills[SkillType.Axe].Level * 0.35));
        player.AddSkillBonus(SkillType.Fist, (sbyte)Math.Abs(player.Skills[SkillType.Axe].Level * 0.35));
        
        actor.DisableShieldDefense();
        actor.IncreaseDamageReceived(15);
        return true;
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