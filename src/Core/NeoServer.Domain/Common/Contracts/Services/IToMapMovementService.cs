using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Creatures.Structs;

namespace NeoServer.Domain.Common.Contracts.Services;

public interface IToMapMovementService
{
    void Move(IPlayer player, MovementParams itemThrow);
}