using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Creatures.Conditions.Enums;

namespace NeoServer.Domain.Creatures.Conditions.Implementations;

public class ConditionInvisible : BaseCondition
{
    public ConditionInvisible(
        uint interval,
        EffectT effect = EffectT.None) : base(interval)
    {
        Effect = effect;
    }

    public override ConditionType Type => ConditionType.Invisible;
    public EffectT Effect { get; }

    public override bool Start(ICreature creature)
    {
        if (!base.Start(creature))
            return false;

        if (creature is not IPlayer player)
            return false;

        player.TurnInvisible();

        EndAction = () => player.TurnVisible();

        return true;
    }
}