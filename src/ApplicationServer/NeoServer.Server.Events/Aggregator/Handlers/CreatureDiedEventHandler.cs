using NeoServer.Game.Common.Aggregator;
using NeoServer.Game.Common.Aggregator.Events;
using NeoServer.Server.Common.Contracts.Scripts;

namespace NeoServer.Server.Events.Aggregator.Handlers;

public class CreatureDiedEventHandler(IScriptGameManager scriptGameManager) : IBaseEventHandler<CreatureDiedEvent>
{
    public void Handle(CreatureDiedEvent @event)
    {
        scriptGameManager.CreatureEventExecuteOnCreatureDeath(@event.Actor, @event.By);
    }
}