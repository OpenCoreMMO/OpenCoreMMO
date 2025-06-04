using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Creatures.Players;

namespace NeoServer.Domain.Common.Contracts.DataStores;

public interface IPlayerOutFitStore : IDataStore<Gender, IEnumerable<IPlayerOutFit>>
{
}