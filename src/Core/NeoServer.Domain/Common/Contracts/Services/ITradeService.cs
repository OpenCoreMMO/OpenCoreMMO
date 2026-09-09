using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.SafeTrade.Validations;

namespace NeoServer.Domain.Common.Contracts.Services;

public interface ITradeService
{
    void Cancel(IPlayer playerCanceling);
    SafeTradeError Request(IPlayer player, IPlayer secondPlayer, IItem item);
}