using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Common.Results;

namespace NeoServer.Domain.Items.Services;

public class ItemUseValidation(IMapTool mapTool, IMap map)
{
    public Result CanUse(IThing item, IPlayer player, IThing target, ItemUseValidationParam param)
    {
        if (target is null || map.GetTile(target.Location) is null) return Result.NotPossible;

        if (item is IItem { AllowFarUse: true })
        {
            if (param.CheckFloor && player.Location.Z != target.Location.Z)
            {
                if (player.Location.Z > target.Location.Z) return Result.Fail(InvalidOperation.FirstGoUpStairs);

                if (player.Location.Z < target.Location.Z) return Result.Fail(InvalidOperation.FirstGoDownStairs);
            }

            if (!player.CanSee(target.Location)) return Result.Fail(InvalidOperation.TooFar);

            var sightLine = param.CheckFloor ? SightLine.CheckSightLineAndFloor : SightLine.CheckSightLine;
            if (param.CheckClearSight && !mapTool.CanThrowObjectTo(player.Location, target.Location, sightLine))
                return Result.Fail(InvalidOperation.CannotThrowThere);

            return Result.Success;
        }

        if (target.Location.X != 0xFFFF)
        {
            if (player.Location.Z != target.Location.Z)
                return player.Location.Z > target.Location.Z
                    ? Result.Fail(InvalidOperation.FirstGoUpStairs)
                    : Result.Fail(InvalidOperation.FirstGoDownStairs);

            if (!player.Location.IsNextTo(target.Location)) return Result.Fail(InvalidOperation.TooFar);
        }

        return Result.Success;
    }
}

public readonly ref struct ItemUseValidationParam
{
    public ItemUseValidationParam(bool checkClearSight, bool checkFloor)
    {
        CheckClearSight = checkClearSight;
        CheckFloor = checkFloor;
    }

    public bool CheckClearSight { get; }
    public bool CheckFloor { get; }
}