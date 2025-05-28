using System;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.Items.Types.Usable;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Game.Common.Results;

namespace NeoServer.Game.Items.Services;

public class ItemUseValidation(IMapTool mapTool, IMap map)
{
    public Result CanUse(IThing item, IPlayer player, IThing target, ItemUseValidationParam param)
    {
        if (target is null || map.GetTile(target.Location) is null)
        {
            return Result.NotPossible;
        }
        
        if (item is IItem { AllowFarUse: true })
        {
            if (param.CheckFloor && player.Location.Z != target.Location.Z)
            {
                if (player.Location.Z > target.Location.Z)
                {
                    return Result.Fail(InvalidOperation.FirstGoUpStairs);
                }

                if (player.Location.Z < target.Location.Z)
                {
                    return Result.Fail(InvalidOperation.FirstGoDownStairs);
                }
            }

            if (!player.CanSee(target.Location))
            {
                return Result.Fail(InvalidOperation.TooFar);
            }

            var sightLine = param.CheckFloor ? SightLine.CheckSightLineAndFloor : SightLine.CheckSightLine;
            if (param.CheckClearSight && !mapTool.CanThrowObjectTo(player.Location, target.Location, sightLine))
            {
                return Result.Fail(InvalidOperation.CannotThrowThere);
            }
        }
        else
        {
            throw new NotImplementedException();
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