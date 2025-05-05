using NeoServer.Game.Common;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Common.Contracts.World.Tiles;
using NeoServer.Game.Common.Helpers;
using NeoServer.Game.Common.Results;

namespace NeoServer.Game.Combat.Services.Attacks;

public class AttackValidation(IMapTool mapTool, IMap map)
{
    public Result Validate(ICombatActor aggressor, IThing target)
    {
        var attackValidationResult = aggressor.CanAttack();
        if (attackValidationResult.Failed)
        {
            return attackValidationResult;
        }

        if (Guard.IsNull(target) || aggressor.Equals(target))
            return Result.NotPossible;
        
        if (!aggressor.CanSee(target.Location) || !aggressor.Location.SameFloorAs(target.Location))
            return Result.Fail(InvalidOperation.CreatureIsNotReachable);

        switch (target)
        {
            case ICombatActor { IsDead: true }:
                return Result.NotPossible;
            case ICombatActor victim when victim.Tile?.ProtectionZone ?? false:
            case ITile { ProtectionZone: true }:
                return Result.Fail(InvalidOperation.CannotAttackPersonInProtectionZone);
            case IItem item:
            {
                var tile = map[item.Location];
                if (tile.ProtectionZone) return Result.Fail(InvalidOperation.CannotAttackPersonInProtectionZone);
                break;
            }
        }

        if (mapTool.SightClearChecker?.Invoke(aggressor.Location, target.Location, true) is false)
        {
            return Result.Fail(InvalidOperation.CannotThrowThere);
        }

        return Result.Success;
    }
}