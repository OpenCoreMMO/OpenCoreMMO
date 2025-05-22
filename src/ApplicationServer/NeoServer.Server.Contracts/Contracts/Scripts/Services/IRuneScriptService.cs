using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items.Types.Runes;

namespace NeoServer.Server.Common.Contracts.Scripts.Services;

public interface IRuneScriptService
{
    bool UseItem(IPlayer player, IAttackRune rune, bool isHotkey);
}