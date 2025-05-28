using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.Items.Types.Runes;

namespace NeoServer.Server.Common.Contracts.Scripts.Services;

public interface IRuneScriptService
{
    bool HasScript(IAttackRune rune);
    bool UseItem(IPlayer player, IThing target, IRune rune, bool isHotkey);
}