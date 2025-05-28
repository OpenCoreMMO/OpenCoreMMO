using System.Linq;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.Spells;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Common.Creatures.Players;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Game.Common.Results;
using NeoServer.Game.Common.Spell;

namespace NeoServer.Game.Combat.Services.Spells;

public class SpellCastValidation(IMapTool mapTool)
{
    public Result CanBeCastBy(ICombatActor caster, IThing target, ISpell spell)
    {
        if (caster is IPlayer aggressorPlayer)
        {
            if (aggressorPlayer.Group.FlagIsEnabled(PlayerFlag.CannotUseSpells))
            {
                return Result.NotPossible;
            }

            if (aggressorPlayer.Group.FlagIsEnabled(PlayerFlag.IgnoreSpellCheck))
            {
                return Result.Success;
            }
        }

        var result = spell.CanBeCastBy(caster, target);
        if (result.Failed)
            return result;

        if (spell.Range.HasValue && mapTool.CanThrowObjectTo(caster.Location, target.Location,
                SightLine.CheckSightLineAndFloor, spell.Range.Value, spell.Range.Value))
        {
            return Result.Fail(InvalidOperation.DestinationOutOfReach);
        }

        if (spell.BlockWalls && spell.NeedsTarget)
        {
            if (mapTool.SightClearChecker?.Invoke(caster.Location, caster.CurrentTarget.Location, true) is false)
            {
                return Result.Fail(InvalidOperation.CannotThrowThere);
            }
        }


        var casterLocation = caster.Location;

        var casterHasNoTarget = spell.HasCooldownGroup((int)MagicGroup.Attack) && caster.CurrentTarget is null;
        var casterNeedsDirection = spell.NeedDirection || spell.CasterNeedsTargetOrDirection;

        //check if the next tile is blocked
        if (casterHasNoTarget && casterNeedsDirection &&
            mapTool.SightClearChecker?.Invoke(caster.Location, casterLocation.AddDirectionStep(caster.Direction, 2),
                    true)
                is false)
        {
            return Result.Fail(InvalidOperation.NotEnoughRoom);
        }

        return Result.Success;
    }
}