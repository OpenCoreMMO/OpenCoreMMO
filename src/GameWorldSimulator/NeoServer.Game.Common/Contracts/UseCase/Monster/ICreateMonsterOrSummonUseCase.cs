using NeoServer.Game.Common.Contracts.Creatures;

namespace NeoServer.Game.Common.Contracts.UseCase.Monster;

public interface ICreateMonsterOrSummonUseCase
{
    IMonster Execute(string name, Location.Structs.Location location, bool extended = false, bool force = false, ICreature master = null);
}