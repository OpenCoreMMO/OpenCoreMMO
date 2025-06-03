using NeoServer.Domain.Common.Combat.Structs;

namespace NeoServer.Domain.Common.Contracts.Items.Types;

public interface IProtection
{
    bool Protect(CombatDamage damage);
}