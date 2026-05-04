using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Creatures.Conditions.Enums;

namespace NeoServer.Domain.Creatures.Conditions.Implementations;

public class OutfitCondition : BaseCondition
{
    private readonly ushort _lookType;
    private readonly byte _head;
    private readonly byte _body;
    private readonly byte _legs;
    private readonly byte _feet;
    private readonly byte _addon;

    public override ConditionType Type => ConditionType.Outfit;

    public OutfitCondition(uint duration, ushort lookType, byte head, byte body, byte legs, byte feet, byte addon)
        : base(duration)
    {
        _lookType = lookType;
        _head = head;
        _body = body;
        _legs = legs;
        _feet = feet;
        _addon = addon;
    }

    public override bool Start(ICreature creature)
    {
        if (!base.Start(creature)) return false;

        if (creature is ICombatActor combatActor)
        {
            combatActor.RemoveCondition(ConditionType.Outfit);
        }

        creature.SetTemporaryOutfit(_lookType, _head, _body, _legs, _feet, _addon);
        EndAction = creature.BackToOldOutfit;

        return true;
    }
}
