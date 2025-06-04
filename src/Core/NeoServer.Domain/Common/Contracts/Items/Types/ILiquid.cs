using NeoServer.Domain.Common.Item;

namespace NeoServer.Domain.Common.Contracts.Items.Types;

public interface ILiquid : IItem
{
    bool IsLiquidPool { get; }

    bool IsLiquidSource { get; }

    bool IsLiquidContainer { get; }
    LiquidColor LiquidColor { get; }
}