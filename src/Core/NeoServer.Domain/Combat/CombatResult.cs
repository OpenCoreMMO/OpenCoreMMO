using NeoServer.Domain.Common.Results;

namespace NeoServer.Domain.Combat;

public readonly ref struct CombatResult(uint totalDamage, Result result)
{
    public uint TotalDamage { get; } = totalDamage;
    public Result Result { get; } = result;

    public static CombatResult Fail(Result result)
    {
        return new CombatResult(0, result);
    }
}