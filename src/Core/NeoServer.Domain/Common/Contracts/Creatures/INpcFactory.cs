using NeoServer.Domain.Common.Contracts.World;

namespace NeoServer.Domain.Common.Contracts.Creatures;

public interface INpcFactory
{
    INpc Create(string name, ISpawnPoint spawn = null);
}