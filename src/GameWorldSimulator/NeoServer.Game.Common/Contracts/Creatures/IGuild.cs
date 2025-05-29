using System.Collections.Generic;
using NeoServer.Game.Common.Contracts.Chats;
using NeoServer.Game.Common.Creatures.Guilds;

namespace NeoServer.Game.Common.Contracts.Creatures;

public interface IGuild: IBankable
{
    ushort Id { get; init; }
    string Name { get; set; }
    IDictionary<ushort, IGuildLevel> GuildLevels { get; set; }
    IChatChannel Channel { get; set; }

    IGuildLevel GetMemberLevel(IPlayer player);
    bool HasMember(IPlayer player);
    string InspectionText(IPlayer player);
}

public interface IGuildLevel
{
    GuildRank Level { get; }
}