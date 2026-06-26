using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServer.Domain.Houses;
using NeoServer.Domain.Houses.AccessList;
using NeoServer.Loaders.Interfaces;

namespace NeoServer.Loaders.Houses;

public class HouseAccessListLoader : ICustomLoader
{
    /// <summary>
    ///     Builds and attaches access lists to a House from raw list data.
    /// </summary>
    public async Task Load(House house, IEnumerable<(uint ListId, string ListText)> accessLists)
    {
        foreach (var (listId, listText) in accessLists)
        {
            var parsed = HouseAccessListParser.Parse(listText);
            var list = BuildAccessList(parsed);
            list.RawText = listText;
            house.SetAccessList(listId, list);
        }
    }

    private static HouseAccessList BuildAccessList(ParseResult parsed)
    {
        var list = new HouseAccessList();

        foreach (var entry in parsed.Entries)
        {
            switch (entry.Kind)
            {
                case ListEntryKind.AllowAll:
                    list.AllowEveryone();
                    break;
                case ListEntryKind.InvitePlayer:
                    list.AddPlayer(entry.Pattern);
                    break;
                case ListEntryKind.ExcludePlayer:
                    list.AddExcludedPlayer(entry.Pattern);
                    break;
                case ListEntryKind.GuildAll:
                    list.AddGuild(entry.Pattern);
                    break;
                case ListEntryKind.GuildRank:
                    list.AddGuildRank(entry.Pattern, entry.Rank);
                    break;
            }
        }

        return list;
    }
}
