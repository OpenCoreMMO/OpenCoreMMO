using NeoServer.Domain.Items.Items.Weapons;

namespace NeoServer.Domain.Common.Contracts.Items.Weapons.Attributes;

public interface INeedsAmmo
{
    bool CanShootAmmunition(Ammo ammo);
}