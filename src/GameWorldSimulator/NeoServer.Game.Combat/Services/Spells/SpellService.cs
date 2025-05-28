using NeoServer.Game.Combat.Services.Attacks.Events;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.Spells;
using NeoServer.Game.Common.Contracts.World;
using Serilog;

namespace NeoServer.Game.Combat.Services.Spells;

public class SpellService(
    SpellCastValidation spellCastValidation,
    IEventAggregator eventAggregator,
    ILogger logger,
    IMap map)
{
    public bool Cast(IPlayer caster, IThing target, ISpell spell, bool isHotkey)
    {
        if (spell is null)
        {
            return false;
        }
        
        var casterLocation = caster.Location;
        
        if (target is null)
        {
            if (spell.NeedsTarget)
            {
                target = caster.CurrentTarget;
            }

            if (spell.NeedDirection || spell.CasterNeedsTargetOrDirection)
            {
                var location = casterLocation.AddDirectionStep(caster.Direction, 1);
                target = map.GetTile(location);
            }
        }

        var result = spellCastValidation.CanBeCastBy(caster, target, spell);

        if (result.Failed)
        {
            eventAggregator.Publish(new SpellFailedToCastEvent(caster, spell, result.Reason));
            return false;
        }

        var invokeResult = spell.Invoke(caster, target, isHotkey);
        if (invokeResult.Failed)
        {
            eventAggregator.Publish(new SpellFailedToCastEvent(caster, spell, invokeResult.Reason));
            return true;
        }

        caster.PostSpellCast(spell);

        return true;
    }
}