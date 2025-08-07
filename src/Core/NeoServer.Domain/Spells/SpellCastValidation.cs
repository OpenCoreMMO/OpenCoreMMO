using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Spells;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Common.Results;
using NeoServer.Domain.Creatures.Player;

namespace NeoServer.Domain.Spells;

public class SpellCastValidation(IMapTool mapTool)
{
    public Result CanBeCastBy(ICombatActor caster, IThing target, ISpell spell)
    {
        if (caster is IPlayer aggressorPlayer)
            if (aggressorPlayer.Group.FlagIsEnabled(PlayerFlag.CannotUseSpells))
                return Result.NotPossible;

        if (spell.IsSelfTarget)
            target = caster;

        var result = spell.CanCast(caster, target);
        if (result.Failed) return result;

        if (spell.Range.HasValue && !mapTool.CanThrowObjectTo(caster.Location, target.Location,
                SightLine.CheckSightLineAndFloor, spell.Range.Value, spell.Range.Value))
            return Result.Fail(InvalidOperation.DestinationOutOfReach);

        if (spell.BlockWalls && spell.NeedsTarget)
            if (mapTool.SightClearChecker?.Invoke(caster.Location, caster.CurrentTarget.Location, true) is false)
                return Result.Fail(InvalidOperation.CannotThrowThere);

        var casterLocation = caster.Location;

        var casterHasNoTarget = spell.HasCooldownGroup((int)MagicGroup.Attack) && caster.CurrentTarget is null;
        var casterNeedsDirection = spell.NeedDirection || spell.NeedCasterTargetOrDirection;

        //check if the next tile is blocked
        if (casterHasNoTarget && casterNeedsDirection &&
            mapTool.SightClearChecker?.Invoke(caster.Location, casterLocation.AddDirectionStep(caster.Direction, 2),
                    true)
                is false)
            return Result.Fail(InvalidOperation.NotEnoughRoom);

        return Result.Success;
    }
}