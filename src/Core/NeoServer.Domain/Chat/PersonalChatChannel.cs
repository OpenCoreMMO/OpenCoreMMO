using NeoServer.Domain.Common.Contracts.Creatures;

namespace NeoServer.Domain.Chat;

public abstract class PersonalChatChannel : ChatChannel
{
    public PersonalChatChannel(ushort id, string name) : base(id, name)
    {
    }

    public new abstract string Name { get; }

    public override bool AddUser(IPlayer player)
    {
        if (users.Count == 1) return false;
        return base.AddUser(player);
    }
}