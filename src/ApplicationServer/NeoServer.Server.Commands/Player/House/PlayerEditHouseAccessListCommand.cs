using System;
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Common.Services;
using NeoServer.Domain.Houses;
using NeoServer.Domain.Houses.Services;
using NeoServer.Domain.Repositories;
using NeoServer.Loaders.Houses;
using NeoServer.Server.Common.Contracts.Commands;

namespace NeoServer.Server.Commands.Player.House;

public class PlayerEditHouseAccessListCommand(
    IHouseStore houseStore,
    IHouseRepository houseRepository,
    HouseAccessListLoader accessListLoader,
    HouseConfiguration houseConfiguration,
    IHouseEviction houseEviction) : ICommand
{
    public void Execute(IPlayer player, uint houseId, uint listId, string text)
    {
        var house = houseStore.GetByHouseId(houseId);
        if (house is null)
        {
            OperationFailService.Send(player, "House not found.");
            return;
        }

        if (!house.CanEditAccessList(listId, player))
        {
            OperationFailService.Send(player, "You cannot edit this access list.");
            return;
        }

        var currentList = house.GetAccessList(listId);
        var rawText = currentList?.RawText ?? string.Empty;

        // Avoid processing when the text hasn't changed
        if (string.Equals(rawText, text, StringComparison.Ordinal))
        {
            return;
        }

        if (text.Length > houseConfiguration.MaxAccessListLength)
        {
            OperationFailService.Send(player, "Access list text exceeds the maximum allowed length.");
            return;
        }

        var lineCount = text.Split('\n').Length;
        if (lineCount > houseConfiguration.MaxAccessListLines)
        {
            OperationFailService.Send(player, "Access list exceeds the maximum allowed number of lines.");
            return;
        }

        if (listId == HouseListId.SubOwnerList &&
            CountSubOwners(text) > houseConfiguration.MaxSubOwnerCount)
        {
            OperationFailService.Send(player,
                $"The sub-owner list may contain at most {houseConfiguration.MaxSubOwnerCount} characters.");
            return;
        }

        // Reload the access list using the loader so parsing is consistent
        accessListLoader
            .Load(house, [(listId, text)])
            .GetAwaiter()
            .GetResult();

        houseRepository.SaveAccessList(houseId, listId, text);

        houseEviction.KickUninvited(house, listId);
    }

    /// <summary>
    ///     Counts how many sub-owners the given list text would grant access to.
    ///     Only entries that invite players (exact names, wildcards, guilds, or
    ///     allow-all) occupy a sub-owner slot; exclusions and comments do not.
    /// </summary>
    private static int CountSubOwners(string text)
    {
        var count = 0;
        foreach (var entry in HouseAccessListParser.Parse(text).Entries)
        {
            if (entry.Kind != ListEntryKind.ExcludePlayer)
            {
                count++;
            }
        }
        return count;
    }
}
