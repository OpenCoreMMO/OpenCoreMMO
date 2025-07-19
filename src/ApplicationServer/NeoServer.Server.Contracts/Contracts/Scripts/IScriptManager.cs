using NeoServer.Server.Common.Contracts.Scripts.Services;

namespace NeoServer.Server.Common.Contracts.Scripts;

public interface IScriptManager
{
    IActionScriptService Actions { get; }
    ICreatureEventsScriptService CreatureEvents { get; }
    IGlobalEventsScriptService GlobalEvents { get; }
    IMoveEventsScriptService MoveEvents { get; }
    ITalkActionScriptService TalkActions { get; }

    void Initialize();
}