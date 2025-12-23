using NeoServer.Domain.Common.Contracts;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Creatures.Events.Player;

namespace NeoServer.Domain.Creatures.Events;

public class CreatureEventSubscriber(
    CreatureTeleportedEventHandler creatureTeleportedEventHandler,
    CreatureMovedEventHandler creatureMovedEventHandler,
    CreatureSayEventHandler creatureSayEventHandler,
    PlayerOpenedContainerEventHandler playerOpenedContainerEventHandler)
    : ICreatureEventSubscriber, IGameEventSubscriber
{
    public void Subscribe(ICreature creature)
    {
        if (creature is IWalkableCreature walkableCreature)
        {
            walkableCreature.OnTeleported += creatureTeleportedEventHandler.Execute;
            walkableCreature.OnCreatureMoved += creatureMovedEventHandler.Execute;
        }

        if (creature is IPlayer player)
            player.Containers.OnOpenedContainer += playerOpenedContainerEventHandler.Execute;

        creature.OnSay += creatureSayEventHandler.Execute;
    }

    public void Unsubscribe(ICreature creature)
    {
        if (creature is IWalkableCreature walkableCreature)
        {
            walkableCreature.OnTeleported -= creatureTeleportedEventHandler.Execute;
            walkableCreature.OnCreatureMoved -= creatureMovedEventHandler.Execute;
        }

        if (creature is IPlayer player)
            player.Containers.OnOpenedContainer -= playerOpenedContainerEventHandler.Execute;

        creature.OnSay -= creatureSayEventHandler.Execute;
    }
}