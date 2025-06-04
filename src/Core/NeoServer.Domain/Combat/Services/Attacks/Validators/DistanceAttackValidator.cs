using NeoServer.Domain.Common.Combat.Structs;

namespace NeoServer.Domain.Combat.Services.Attacks.Validators;

public static class DistanceAttackValidator
{
    public static bool IsValid(AttackInput attackInput)
    {
        if (!attackInput.HasTarget) return true;

        var aggressor = attackInput.Aggressor;
        var target = attackInput.Target;

        var sqmDistance = target.Location.GetMaxSqmDistance(aggressor.Location);

        var maxRange = Math.Max(attackInput.Parameters.Range ?? 1, (byte)1);

        return maxRange >= sqmDistance;
    }
}