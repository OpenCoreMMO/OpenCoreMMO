using NeoServer.Game.Common.Combat;

namespace NeoServer.Game.Common.Contracts.Items;

public interface IHasElementalDamage
{
    ElementalDamage ElementalDamage { get; }
}