using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Creatures.Player;

namespace NeoServer.Domain.Common.Contracts.DataStores;

public interface IPlayerOutFitStore : IDataStore<Gender, IEnumerable<IPlayerOutFit>>
{
}