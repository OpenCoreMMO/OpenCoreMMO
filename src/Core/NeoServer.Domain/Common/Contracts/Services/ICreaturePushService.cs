using NeoServer.Domain.Common.Contracts.Creatures;

namespace NeoServer.Domain.Common.Contracts.Services;

public interface ICreaturePushService
{
    void PushCreature(IPlayer player, ICreature target, NeoServer.Domain.Common.Location.Structs.Location destination);
}