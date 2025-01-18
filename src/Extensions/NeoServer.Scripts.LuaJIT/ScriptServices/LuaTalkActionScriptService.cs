using NeoServer.Game.Common.Chats;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Scripts.LuaJIT.Interfaces;
using NeoServer.Server.Common.Contracts.Scripts.Services;
using Serilog;

namespace NeoServer.Scripts.LuaJIT.ScriptServices;

public class LuaTalkActionScriptService : ITalkActionScriptService
{
    #region Members

    #endregion

    #region Dependency Injections

    /// <summary>
    /// A reference to the <see cref="ILogger"/> instance in use.
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// A reference to the <see cref="ITalkActions"/> instance in use.
    /// </summary>
    private readonly ITalkActions _talkActions;

    #endregion

    #region Constructors

    public LuaTalkActionScriptService(
        ILogger logger,
        ITalkActions talkActions)
    {
        _logger = logger;
        _talkActions = talkActions;
    }

    #endregion

    #region Public Methods 

    public bool Say(IPlayer player, SpeechType type, string words)
    {
        var wordsSeparator = " ";
        var talkactionWords = words.Contains(wordsSeparator) ? words.Split(" ") : [words];

        if (!talkactionWords.Any())
            return false;

        var talkAction = _talkActions.GetTalkAction(talkactionWords[0]);

        if (talkAction == null)
            return false;

        var parameter = talkactionWords.Length > 1
                    ? string.Join(wordsSeparator, talkactionWords.Skip(1))
                    : "";

        return talkAction.ExecuteSay(player, talkactionWords[0], parameter, type);
    }

    #endregion
}
