using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;

namespace NeoServer.Server.Common.Contracts.Scripts.Services;

public interface ICreatureEventsScriptService
{
    void ExtendedOpcodeHandle(IPlayer player, byte opcode, string buffer);
    void ExecuteOnCreatureDeath(ICombatActor actor, IThing by);
}