using NeoServer.Domain.Common.Combat;

namespace NeoServer.Domain.Common.Contracts.Items;

public interface IHasElementalDamage
{
    ElementalDamage ElementalDamage { get; }
}