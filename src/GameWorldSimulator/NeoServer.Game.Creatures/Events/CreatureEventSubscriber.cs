using NeoServer.Game.Common.Contracts;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Creatures.Events.Player;

namespace NeoServer.Game.Creatures.Events;

public class CreatureEventSubscriber : ICreatureEventSubscriber, IGameEventSubscriber
{
    private readonly CreatureDamagedEventHandler creatureDamagedEventHandler;
    private readonly CreatureMovedEventHandler creatureMovedEventHandler;
    private readonly CreaturePropagatedAttackEventHandler creaturePropagatedAttackEventHandler;
    private readonly CreatureSayEventHandler creatureSayEventHandler;
    private readonly CreatureTeleportedEventHandler creatureTeleportedEventHandler;
    private readonly PlayerDisappearedEventHandler playerDisappearedEventHandler;
    private readonly PlayerLoggedInEventHandler playerLoggedInEventHandler;
    private readonly PlayerLoggedOutEventHandler playerLoggedOutEventHandler;
    private readonly PlayerOpenedContainerEventHandler playerOpenedContainerEventHandler;

    public CreatureEventSubscriber(
        CreatureDamagedEventHandler creatureDamagedEventHandler,
        CreaturePropagatedAttackEventHandler creaturePropagatedAttackEventHandler,
        CreatureTeleportedEventHandler creatureTeleportedEventHandler,
        PlayerDisappearedEventHandler playerDisappearedEventHandler,
        CreatureMovedEventHandler creatureMovedEventHandler, PlayerLoggedInEventHandler playerLoggedInEventHandler,
        PlayerLoggedOutEventHandler playerLoggedOutEventHandler,
        CreatureSayEventHandler creatureSayEventHandler,
        PlayerOpenedContainerEventHandler playerOpenedContainerEventHandler)
    {
        this.creatureDamagedEventHandler = creatureDamagedEventHandler;
        this.creaturePropagatedAttackEventHandler = creaturePropagatedAttackEventHandler;
        this.creatureTeleportedEventHandler = creatureTeleportedEventHandler;
        this.playerDisappearedEventHandler = playerDisappearedEventHandler;
        this.creatureMovedEventHandler = creatureMovedEventHandler;
        this.playerLoggedInEventHandler = playerLoggedInEventHandler;
        this.playerLoggedOutEventHandler = playerLoggedOutEventHandler;
        this.creatureSayEventHandler = creatureSayEventHandler;
        this.playerOpenedContainerEventHandler = playerOpenedContainerEventHandler;
    }

    public void Subscribe(ICreature creature)
    {
        if (creature is ICombatActor combatActor)
        {
            combatActor.OnInjured += creatureDamagedEventHandler.Execute;
            combatActor.OnPropagateAttack += creaturePropagatedAttackEventHandler.Execute;
        }

        if (creature is IWalkableCreature walkableCreature)
        {
            walkableCreature.OnTeleported += creatureTeleportedEventHandler.Execute;
            walkableCreature.OnCreatureMoved += creatureMovedEventHandler.Execute;
        }

        if (creature is IPlayer player)
        {
            player.OnLoggedOut += playerDisappearedEventHandler.Execute;
            player.OnLoggedIn += playerLoggedInEventHandler.Execute;
            player.OnLoggedOut += playerLoggedOutEventHandler.Execute;
            player.Containers.OnOpenedContainer += playerOpenedContainerEventHandler.Execute;
        }

        creature.OnSay += creatureSayEventHandler.Execute;
    }

    public void Unsubscribe(ICreature creature)
    {
        if (creature is ICombatActor combatActor)
        {
            combatActor.OnInjured -= creatureDamagedEventHandler.Execute;
            combatActor.OnPropagateAttack -= creaturePropagatedAttackEventHandler.Execute;
        }

        if (creature is IWalkableCreature walkableCreature)
        {
            walkableCreature.OnTeleported -= creatureTeleportedEventHandler.Execute;
            walkableCreature.OnCreatureMoved -= creatureMovedEventHandler.Execute;
        }

        if (creature is IPlayer player)
        {
            player.OnLoggedOut -= playerDisappearedEventHandler.Execute;
            player.OnLoggedIn -= playerLoggedInEventHandler.Execute;
            player.OnLoggedOut -= playerLoggedOutEventHandler.Execute;
            player.Containers.OnOpenedContainer -= playerOpenedContainerEventHandler.Execute;
        }
        
        creature.OnSay -= creatureSayEventHandler.Execute;
    }
}