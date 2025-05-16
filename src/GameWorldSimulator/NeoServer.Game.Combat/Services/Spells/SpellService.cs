using NeoServer.Game.Combat.Services.Attacks.Events;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Spells;

namespace NeoServer.Game.Combat.Services.Spells;

public class SpellService(
    SpellCastValidation spellCastValidation,
    IEventAggregator eventAggregator)
{
    public bool Cast(IPlayer caster, ISpell spell)
    {
        var result = spellCastValidation.CanBeCastBy(caster, spell);

        if (result is not InvalidOperation.None)
        {
            eventAggregator.Publish(new SpellFailedToCastEvent(caster, spell, result));
            return false;
        }

        if (!spell.Invoke(caster, spell.Words, out var error))
        {
            eventAggregator.Publish(new SpellFailedToCastEvent(caster, spell, error));
            return true;
        }

        caster.PostSpellCast(spell);

        return true;
    }
}