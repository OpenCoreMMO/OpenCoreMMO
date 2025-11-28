using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Spells;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Spells.Events;
using NeoServer.Domain.World.Models.Tiles;

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

        target ??= GetTarget(caster, spell, casterLocation);

        var result = spellCastValidation.CanBeCastBy(caster, target, spell);

        if (result.Failed && caster is IPlayer)
        {
            eventAggregator.InvokeEvent(new SpellFailedToCastEvent(caster, spell, result.Reason));
            return false;
        }

        var invokeResult = spell.Invoke(caster, target, isHotkey);

        if (invokeResult.Failed && caster is IPlayer) return true;

        if (caster is IPlayer player) player.PostSpellCast(spell);

        return true;
    }

    private IThing GetTarget(ICombatActor caster, ISpell spell, Location casterLocation)
    {
        if (spell.NeedsTarget) return caster.CurrentTarget;

        if (spell.NeedDirection || spell.NeedCasterTargetOrDirection)
        {
            if (spell.NeedCasterTargetOrDirection && caster.CurrentTarget is not null) return caster.CurrentTarget;

            var location = casterLocation.AddDirectionStep(caster.Direction);
            return map.GetTile(location) ?? new EmptyTile(location);
        }

        return null;
    }
}