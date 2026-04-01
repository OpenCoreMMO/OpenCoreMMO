using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items.Types;

namespace NeoServer.Domain.Common.Contracts.Items;

public interface IEquipment : IDecay, ISkillBonus, IDressable, IProtection, ITransformableEquipment, IChargeable,
    IHasDecay,
    IEquipmentRequirement
{
    IPlayer PlayerDressing { get; }
}