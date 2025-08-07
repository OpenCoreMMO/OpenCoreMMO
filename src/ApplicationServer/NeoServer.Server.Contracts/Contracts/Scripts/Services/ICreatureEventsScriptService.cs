using NeoServer.Domain.Common.Contracts.Creatures;

namespace NeoServer.Server.Common.Contracts.Scripts.Services;

public interface ICreatureEventsScriptService
{
    void ExtendedOpcodeHandle(IPlayer player, byte opcode, string buffer);
}