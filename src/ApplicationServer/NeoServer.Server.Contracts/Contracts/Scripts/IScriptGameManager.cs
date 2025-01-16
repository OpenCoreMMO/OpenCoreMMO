using NeoServer.Server.Common.Contracts.Scripts.Services;

namespace NeoServer.Server.Common.Contracts.Scripts;

public interface IScriptGameManager
{
    IActionScripts Actions { get; }
    ICreatureEventsScripts CreatureEvents { get; }
    IGlobalEventsScripts GlobalEvents { get; }
    IMoveEventsScripts MoveEvents { get; }
    ITalkActionScripts TalkActions { get; }

    void Initialize();
}