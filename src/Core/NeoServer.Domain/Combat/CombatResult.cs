using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Results;

namespace NeoServer.Domain.Combat;

public readonly ref struct CombatResult(uint totalDamage, Result result)
{
    public uint TotalDamage { get; } = totalDamage;
    public Result Result { get; } = result;
    public static CombatResult Fail(Result result) => new(0, result);
}