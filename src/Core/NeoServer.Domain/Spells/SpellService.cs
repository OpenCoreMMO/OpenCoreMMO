using NeoServer.Domain.Chat;
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Services;
using NeoServer.Domain.Common.Contracts.Spells;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Spells.Events;
using NeoServer.Domain.World.Models.Tiles;

namespace NeoServer.Domain.Spells;

public class SpellService(
    SpellCastValidation spellCastValidation,
    IEventAggregator eventAggregator,
    ICreatureSpeechService creatureSpeechService,
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

        if (caster is IPlayer player)
        {
            player.PostSpellCast(spell);
            
            if (!spell.ShouldSay) return true;

            if (!string.IsNullOrWhiteSpace(spell.Words))
            {
                creatureSpeechService.Speak(caster, spell.Words, SpeechType.MonsterSay);
            }
        }
        
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