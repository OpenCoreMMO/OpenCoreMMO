using NeoServer.Domain.Common.Contracts.Creatures;

namespace NeoServer.Domain.Houses.Services;

public interface IHouseTradeService
{
    HouseTradeResult StartTrade(House house, IPlayer seller, IPlayer partner);
}
