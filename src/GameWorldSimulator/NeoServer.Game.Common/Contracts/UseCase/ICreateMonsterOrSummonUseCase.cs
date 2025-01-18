using NeoServer.Game.Common.Contracts.Creatures;

namespace NeoServer.Game.Common.Contracts.UseCase;

public interface ICreateMonsterOrSummonUseCase {
    IMonster Invoke(string name, Location.Structs.Location location, bool extended = false, bool force = false, ICreature master = null);
}