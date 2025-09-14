using NeoServer.Domain.Common.Contracts.Creatures;

namespace NeoServer.Domain.Common.Contracts.Services;

public interface ITradeService
{
    void Cancel(IPlayer playerCanceling);
}