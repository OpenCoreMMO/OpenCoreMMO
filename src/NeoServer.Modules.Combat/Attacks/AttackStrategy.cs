using NeoServer.Game.Combat;
using NeoServer.Game.Common.Results;

namespace NeoServer.Modules.Combat.Attacks;

public interface IAttackStrategy
{
    string Name { get; }
    Result Execute(in AttackInput attackInput);
}
public abstract class AttackStrategy : IAttackStrategy
{
    public abstract string Name { get; }

    public Result Execute(in AttackInput attackInput)
    {
        return Attack(attackInput);
    }

    protected abstract Result Attack(in AttackInput attackInput);
}