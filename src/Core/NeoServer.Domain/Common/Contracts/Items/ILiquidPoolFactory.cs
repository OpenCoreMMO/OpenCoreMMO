using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Common.Item;

namespace NeoServer.Domain.Common.Contracts.Items;

public interface ILiquidPoolFactory : IFactory
{
    ILiquid Create(Location.Structs.Location location, LiquidColor color);
    ILiquid CreateDamageLiquidPool(Location.Structs.Location location, LiquidColor color);
}