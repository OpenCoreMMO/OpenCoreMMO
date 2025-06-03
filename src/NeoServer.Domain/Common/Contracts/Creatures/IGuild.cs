using NeoServer.Domain.Common.Contracts.Chats;
using NeoServer.Domain.Common.Creatures.Guilds;

namespace NeoServer.Domain.Common.Contracts.Creatures;

public interface IGuild : IBankable
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