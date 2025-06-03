using NeoServer.Domain.Common.Chats;
using NeoServer.Domain.Common.Contracts.Creatures;

namespace NeoServer.Domain.Common.Contracts.Chats;

public delegate void AddMessage(ISociableCreature creature, IChatChannel channel, SpeechType speechType,
    string message);

public interface IChatChannel
{
    ushort Id { get; }
    string Name { get; }
    IEnumerable<IUserChat> Users { get; }
    string Description { get; }
    bool Opened { get; }

    event AddMessage OnMessageAdded;

    bool AddUser(IPlayer player);
    SpeechType GetTextColor(IPlayer player);
    bool HasUser(IPlayer player);
    bool PlayerCanJoin(IPlayer player);
    bool PlayerCanWrite(IPlayer player);
    bool PlayerIsMuted(IPlayer player, out string cancelMessage);
    bool RemoveUser(IPlayer player);

    bool WriteMessage(ISociableCreature creature, string message, out string cancelMessage,
        SpeechType speechType = SpeechType.None);

    bool WriteMessage(string message, out string cancelMessage, SpeechType speechType = SpeechType.None);
}

public interface IUserChat
{
    bool Removed { get; }
    bool IsMuted { get; }
    IPlayer Player { get; init; }
}