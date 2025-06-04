using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Items.Items;

namespace NeoServer.Domain.Common.Contracts.Items;

public interface ILiquidPoolFactory : IFactory
{
    LiquidPool Create(Location.Structs.Location location, LiquidColor color);
    LiquidPool CreateDamageLiquidPool(Location.Structs.Location location, LiquidColor color);
}