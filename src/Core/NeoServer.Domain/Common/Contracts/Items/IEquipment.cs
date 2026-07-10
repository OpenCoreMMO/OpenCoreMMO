using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items.Types;

namespace NeoServer.Domain.Common.Contracts.Items;

public interface IEquipment : IDecay, ISkillBonus, IDressable, IProtection, ITransformableEquipment,
    IHasDecay,
    IEquipmentRequirement
{
    IPlayer PlayerDressing { get; }
}