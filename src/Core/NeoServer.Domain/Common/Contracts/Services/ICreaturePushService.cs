using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.World.Tiles;

namespace NeoServer.Domain.Common.Contracts.Services;

public interface ICreaturePushService
{
    void PushCreature(IPlayer player, ICreature target, ITile toTile);
}