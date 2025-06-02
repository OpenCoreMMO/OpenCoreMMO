using NeoServer.Game.Common.Contracts.Items.Types.Body;

namespace NeoServer.Game.Common.Contracts.Items.Weapons.Attributes;

public interface INeedsAmmo
{
    bool CanShootAmmunition(IAmmo ammo);
}