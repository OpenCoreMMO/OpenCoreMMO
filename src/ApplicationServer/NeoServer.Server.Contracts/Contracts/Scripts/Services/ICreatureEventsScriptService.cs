using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;

namespace NeoServer.Server.Common.Contracts.Scripts.Services;

public interface ICreatureEventsScriptService
{
    void ExtendedOpcodeHandle(IPlayer player, byte opcode, string buffer);
    void ExecuteOnCreatureDeath(ICombatActor actor, IThing by);
}