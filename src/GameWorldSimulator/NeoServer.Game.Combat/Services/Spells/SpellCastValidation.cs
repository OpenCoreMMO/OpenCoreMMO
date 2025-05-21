using System.Linq;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Spells;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Common.Spell;

namespace NeoServer.Game.Combat.Services.Spells;

public class SpellCastValidation(IMapTool mapTool)
{
    public InvalidOperation CanBeCastBy(ICombatActor caster, ISpell spell)
    {
        if (!spell.CanBeCastBy(caster, out var error)) return error;

        if (spell.BlockWalls && spell.NeedsTarget)
        {
            if (mapTool.SightClearChecker?.Invoke(caster.Location, caster.CurrentTarget.Location, true) is false)
            {
                return InvalidOperation.CannotThrowThere;
            }
        }
        
        if (spell.Groups is null)
            return InvalidOperation.None;
        
        var casterLocation = caster.Location;

        var casterHasNoTarget = spell.Groups.Contains(MagicGroup.Attack) && caster.CurrentTarget is null;
        var casterNeedsDirection = spell.NeedDirection || spell.CasterNeedsTargetOrDirection;
        
        //check if the next tile is blocked
        if (casterHasNoTarget && casterNeedsDirection &&
            mapTool.SightClearChecker?.Invoke(caster.Location, casterLocation.AddDirectionStep(caster.Direction, 2), true)
                is false)
        {
            return InvalidOperation.NotEnoughRoom;
        }

        return InvalidOperation.None;
    }
}