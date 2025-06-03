using System.Collections.Generic;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Common.Creatures.Players;

namespace NeoServer.Data.InMemory.DataStores;

public class PlayerOutFitStore : DataStore<PlayerOutFitStore, Gender, IEnumerable<IPlayerOutFit>>, IPlayerOutFitStore
{
}