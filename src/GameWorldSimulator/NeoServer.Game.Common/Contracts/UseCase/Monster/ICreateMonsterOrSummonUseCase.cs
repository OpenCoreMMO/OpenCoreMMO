using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.World;

namespace NeoServer.Game.Common.Contracts.UseCase.Monster;

public interface ICreateMonsterOrSummonUseCase
{
    IMonster Execute(string name, ISpawnPoint spawn, Location.Structs.Location location = default, bool extended = false, bool force = false, ICreature master = null);
}