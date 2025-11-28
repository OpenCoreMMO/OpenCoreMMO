using NeoServer.Domain.Common.Contracts;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Creatures.Events.Player;

namespace NeoServer.Domain.Creatures.Events;

public class CreatureEventSubscriber : ICreatureEventSubscriber, IGameEventSubscriber
{
    private readonly CreatureMovedEventHandler creatureMovedEventHandler;
    private readonly CreaturePropagatedAttackEventHandler creaturePropagatedAttackEventHandler;
    private readonly CreatureSayEventHandler creatureSayEventHandler;
    private readonly CreatureTeleportedEventHandler creatureTeleportedEventHandler;
    private readonly PlayerOpenedContainerEventHandler playerOpenedContainerEventHandler;

    public CreatureEventSubscriber(
        CreaturePropagatedAttackEventHandler creaturePropagatedAttackEventHandler,
        CreatureTeleportedEventHandler creatureTeleportedEventHandler,
        CreatureMovedEventHandler creatureMovedEventHandler,
        CreatureSayEventHandler creatureSayEventHandler,
        PlayerOpenedContainerEventHandler playerOpenedContainerEventHandler)
    {
        this.creaturePropagatedAttackEventHandler = creaturePropagatedAttackEventHandler;
        this.creatureTeleportedEventHandler = creatureTeleportedEventHandler;
        this.creatureMovedEventHandler = creatureMovedEventHandler;
        this.creatureSayEventHandler = creatureSayEventHandler;
        this.playerOpenedContainerEventHandler = playerOpenedContainerEventHandler;
    }

    public void Subscribe(ICreature creature)
    {
        if (creature is ICombatActor combatActor)
            combatActor.OnPropagateAttack += creaturePropagatedAttackEventHandler.Execute;

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
        if (creature is ICombatActor combatActor)
            combatActor.OnPropagateAttack -= creaturePropagatedAttackEventHandler.Execute;

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