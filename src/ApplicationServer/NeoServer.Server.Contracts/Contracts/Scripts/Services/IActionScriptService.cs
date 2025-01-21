using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Location.Structs;

namespace NeoServer.Server.Common.Contracts.Scripts.Services;

public interface IActionScriptService
{
    bool HasAction(IItem item);
    bool UseItem(IPlayer player, Location pos, byte stackpos, byte index, IItem item, IThing target = null);
    bool UseItem(IPlayer player, Location fromPos, Location toPos, byte toStackPos, IItem item, IThing target = null, bool isHotkey = false);
}