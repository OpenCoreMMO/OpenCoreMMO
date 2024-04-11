﻿using NeoServer.Game.Common.Contracts.Items.Weapons;
using NeoServer.Game.Common.Item;

namespace NeoServer.Game.Common.Contracts.Items.Types.Body;

public interface IAmmo : ICumulative, IBodyEquipmentEquipment, IHasAttack
{
    byte ExtraHitChance { get; }
    AmmoType AmmoType { get; }
    ShootType ShootType { get; }
    bool HasElementalDamage { get; }
    void Throw();
}