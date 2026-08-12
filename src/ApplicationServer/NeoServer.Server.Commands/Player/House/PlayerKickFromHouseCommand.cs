using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Common.Services;
using NeoServer.Domain.Houses.Services;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Commands;

namespace NeoServer.Server.Commands.Player.House;

public class PlayerKickFromHouseCommand(
    IGameCreatureManager creatureManager,
    IHouseStore houseStore,
    IHouseService houseService) : ICommand
{
    public void Execute(IPlayer caster, string targetPlayerName)
    {
        if (!creatureManager.TryGetPlayer(targetPlayerName, out var target))
        {
            OperationFailService.Send(caster, "Player not found.");
            return;
        }

        var house = houseStore.GetByTile(target.Tile);
        if (house is null)
        {
            OperationFailService.Send(caster, "Target is not inside a house.");
            return;
        }

        if (!houseService.KickPlayer(house, caster, target))
        {
            OperationFailService.Send(caster, "You cannot kick this player.");
        }
    }
}
