using NeoServer.Game.Common.Chats;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Location.Structs;

namespace NeoServer.Application.Common.Contracts.Scripts;

public interface ILuaGameManager
{
    void Start();
    bool PlayerSaySpell(IPlayer player, SpeechType type, string text);
    bool PlayerUseItem(IPlayer player, Location pos, byte stackpos, byte index, IItem item);
    bool PlayerUseItemWithCreature(IPlayer player, Location fromPos, byte fromStackPos, ICreature creature, IItem item);
    bool PlayerUseItemEx(IPlayer player, Location fromPos, Location toPos, byte toStackPos, IItem item, bool isHotkey, IThing target);
}
