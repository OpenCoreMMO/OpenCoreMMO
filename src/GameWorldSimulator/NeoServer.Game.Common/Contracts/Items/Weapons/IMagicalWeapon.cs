using NeoServer.Game.Common.Contracts.Items.Types.Body;

namespace NeoServer.Game.Common.Contracts.Items.Weapons;

public interface IMagicalWeapon: IDistanceWeapon
{
    public ushort MaxHitChance { get; }

    public ushort ManaConsumption { get; }
}