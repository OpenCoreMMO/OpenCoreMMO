using NeoServer.Game.Combat;
using NeoServer.Game.Combat.Attacks.MeleeAttack;
using NeoServer.Game.Combat.Attacks.Validation;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Results;

namespace NeoServer.Modules.Combat.Attacks.DistanceAttack;

public sealed class DistanceAttackValidation(AttackValidation attackValidation) : IAttackValidation
{
    public Result Validate(AttackInput parameter)
    {
        ArgumentNullException.ThrowIfNull(parameter.Aggressor);

        if (parameter.Aggressor is not ICombatActor aggressor) return Result.NotApplicable;

        var result = attackValidation.Validate(aggressor, parameter.Target);
        if (result.Failed) return result;

        if (aggressor.Location.GetMaxSqmDistance(parameter.Target.Location) <= parameter.Parameters.Range)
            return Result.Fail(InvalidOperation.TooFar);

        return Result.Success;
    }
}