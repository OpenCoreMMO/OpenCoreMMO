using NeoServer.Domain.Combat.Services.Attacks.Events;
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Spells;
using NeoServer.Domain.Common.Contracts.World;

namespace NeoServer.Domain.Spells;

public class SpellService(
    SpellCastValidation spellCastValidation,
    IEventAggregator eventAggregator,
    IMap map)
{
    public bool Cast(ICombatActor caster, IThing target, ISpell spell, bool isHotkey)
    {
        if (spell is null) return false;

        var casterLocation = caster.Location;

        if (target is null)
        {
            if (spell.NeedsTarget) target = caster.CurrentTarget;

            if (spell.NeedDirection || spell.NeedCasterTargetOrDirection)
            {
                var location = casterLocation.AddDirectionStep(caster.Direction);
                target = map.GetTile(location);
            }
        }

        var result = spellCastValidation.CanBeCastBy(caster, target, spell);

        if (result.Failed && caster is IPlayer)
        {
            eventAggregator.Publish(new SpellFailedToCastEvent(caster, spell, result.Reason));
            return false;
        }

        var invokeResult = spell.Invoke(caster, target, isHotkey);

        if (invokeResult.Failed && caster is IPlayer)
        {
            eventAggregator.Publish(new SpellFailedToCastEvent(caster, spell, invokeResult.Reason));
            return true;
        }

        if (caster is IPlayer player) player.PostSpellCast(spell);

        return true;
    }
}