using NeoServer.Game.Common.Combat.Structs;

namespace NeoServer.Game.Common.Contracts.Items.Types;

public interface IProtection
{
    bool Protect(CombatDamage damage);
}