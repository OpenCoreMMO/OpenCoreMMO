using System;
using NeoServer.Game.Common.Combat.Structs;

namespace NeoServer.Game.Combat.Services.Attacks.Validators;

public static  class DistanceAttackValidator
{
    public static bool IsValid(AttackInput attackInput)
    {
        if (!attackInput.HasTarget) return true;
        
        var aggressor = attackInput.Aggressor;
        var target = attackInput.Target;

        var sqmDistance = target.Location.GetMaxSqmDistance(aggressor.Location);

        var maxRange = Math.Max(attackInput.Parameters.Range, (byte)1);

        return maxRange >= sqmDistance;
    }
}