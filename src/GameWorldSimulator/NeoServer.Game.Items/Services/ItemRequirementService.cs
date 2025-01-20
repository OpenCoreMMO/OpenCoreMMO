using NeoServer.Game.Common;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.Services;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Creatures.Players;
using NeoServer.Game.Common.Results;

namespace NeoServer.Game.Items.Services;

public class ItemRequirementService : IItemRequirementService
{
    public Result PlayerCanUseItem(IPlayer player, IItem item, Requirement requirement)
    {
        if (player.Level < requirement.MinLevel) return Result.Fail(InvalidOperation.NotEnoughLevel);

        var hasRequiredVocation = PlayerHasRequiredVocation(player, requirement);
        if (!hasRequiredVocation) return Result.Fail(InvalidOperation.VocationCannotUseSpell);

        if ((player.Skills[SkillType.Magic]?.Level ?? 0) < requirement.MinMagicLevel)
            return Result.Fail(InvalidOperation.NotEnoughMagicLevel);

        if (player.PremiumTime <= 0 && requirement.RequirePremiumTime)
            return Result.Fail(InvalidOperation.PremiumTimeIsRequired);

        return Result.Success;
    }

    public Result PlayerCanEquipItem(IPlayer player, IItem item, Requirement requirement)
    {
        if (player.Level < requirement.MinLevel) return Result.Fail(InvalidOperation.NotEnoughLevel);

        if (player.PremiumTime <= 0 && requirement.RequirePremiumTime)
            return Result.Fail(InvalidOperation.PremiumTimeIsRequired);

        var hasRequiredVocation = PlayerHasRequiredVocation(player, requirement);
        if (!hasRequiredVocation) return Result.Fail(InvalidOperation.VocationCannotUseSpell);

        if ((player.Skills[SkillType.Magic]?.Level ?? 0) < requirement.MinMagicLevel)
            return Result.Fail(InvalidOperation.NotEnoughMagicLevel);

        if (requirement.Slot is not Slot.None && item.Metadata.BodyPosition != requirement.Slot)
            return Result.Fail(InvalidOperation.CannotDress);

        return Result.Success;
    }

    private static bool PlayerHasRequiredVocation(IPlayer player, Requirement requirement)
    {
        if (requirement.RequiredVocations is null || requirement.RequiredVocations.Length == 0) return true;

        foreach (var requiredVocation in requirement.RequiredVocations)
        {
            if (player.VocationType == requiredVocation) return true;
        }

        return false;
    }
}