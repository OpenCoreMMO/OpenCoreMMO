using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;

namespace NeoServer.Game.Common.Aggregator.Events;

public record CreatureDiedEvent() : IBaseEvent
{
    public ICombatActor Actor { get; set; }
    public IThing By { get; set; }

    public void Reset()
    {
        Actor = null;
        By = null;
    }
}