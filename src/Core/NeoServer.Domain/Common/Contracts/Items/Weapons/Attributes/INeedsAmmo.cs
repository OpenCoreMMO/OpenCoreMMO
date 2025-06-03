using NeoServer.Domain.Common.Contracts.Items.Types.Body;

namespace NeoServer.Domain.Common.Contracts.Items.Weapons.Attributes;

public interface INeedsAmmo
{
    bool CanShootAmmunition(IAmmo ammo);
}