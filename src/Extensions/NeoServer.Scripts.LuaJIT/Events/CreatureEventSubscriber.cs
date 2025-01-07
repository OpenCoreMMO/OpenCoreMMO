using NeoServer.Game.Common.Contracts;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Scripts.LuaJIT.Events.Creatures;
using NeoServer.Scripts.LuaJIT.Events.Players;

namespace NeoServer.Scripts.LuaJIT;

public class CreatureEventSubscriber : ICreatureEventSubscriber, IGameEventSubscriber
{
    private readonly CreatureOnDeathEventHandler _creatureOnDeathEventHandler;
    private readonly CreatureOnThinkEventHandler _creatureOnThinkEventHandler;
    private readonly CreatureOnKillEventHandler _creatureOnKillEventHandler;
    private readonly CreatureOnPrepareDeathEventHandler _creatureOnPrepareDeathEventHandler;
    private readonly CreatureOnHealthChangeEventHandler _creatureOnHealthChangeEventHandler;
    private readonly CreatureOnManaChangeEventHandler _creatureOnManaChangeEventHandler;

    private readonly PlayerOnLoginEventHandler _playerOnLoginEventHandler;
    private readonly PlayerOnLogoutEventHandler _playerOnLogoutEventHandler;
    private readonly PlayerOnAdvanceEventHandler _playerOnAdvanceEventHandler;
    private readonly PlayerOnTextEditEventHandler _playerOnTextEditEventHandler;

    public CreatureEventSubscriber(
        CreatureOnDeathEventHandler creatureOnDeathEventHandler,
        CreatureOnThinkEventHandler creatureOnThinkEventHandler,
        CreatureOnKillEventHandler creatureOnKillEventHandler,
        CreatureOnPrepareDeathEventHandler creatureOnPrepareDeathEventHandler,
        CreatureOnHealthChangeEventHandler creatureOnHealthChangeEventHandler,
        CreatureOnManaChangeEventHandler creatureOnManaChangeEventHandler,
        PlayerOnLoginEventHandler playerOnLoginEventHandler,
        PlayerOnLogoutEventHandler playerOnLogoutEventHandler,
        PlayerOnAdvanceEventHandler playerOnAdvanceEventHandler,
        PlayerOnTextEditEventHandler playerOnTextEditEventHandler)
    {
        _creatureOnDeathEventHandler = creatureOnDeathEventHandler;
        _creatureOnThinkEventHandler = creatureOnThinkEventHandler;
        _creatureOnKillEventHandler = creatureOnKillEventHandler;
        _creatureOnPrepareDeathEventHandler = creatureOnPrepareDeathEventHandler;
        _creatureOnHealthChangeEventHandler = creatureOnHealthChangeEventHandler;
        _creatureOnManaChangeEventHandler = creatureOnManaChangeEventHandler;

        _playerOnLoginEventHandler = playerOnLoginEventHandler;
        _playerOnLogoutEventHandler = playerOnLogoutEventHandler;
        _playerOnAdvanceEventHandler = playerOnAdvanceEventHandler;
        _playerOnTextEditEventHandler = playerOnTextEditEventHandler;
    }

    public void Subscribe(ICreature creature)
    {
        creature.OnThink += _creatureOnThinkEventHandler.Execute;

        if (creature is ICombatActor actor)
        {
            actor.OnDeath += _creatureOnDeathEventHandler.Execute;
            actor.OnKill += _creatureOnKillEventHandler.Execute;
            actor.OnBeforeDeath += _creatureOnPrepareDeathEventHandler.Execute;
            actor.OnHealthChanged += _creatureOnHealthChangeEventHandler.Execute;
            actor.OnManaChanged += _creatureOnManaChangeEventHandler.Execute;
        }

        if (creature is IPlayer player)
        {
            player.OnLoggedIn += _playerOnLoginEventHandler.Execute;
            player.OnLoggedOut += _playerOnLogoutEventHandler.Execute;
            player.OnLevelAdvanced += _playerOnAdvanceEventHandler.Execute;
            player.OnWroteText += _playerOnTextEditEventHandler.Execute;
        }
    }

    public void Unsubscribe(ICreature creature)
    {
        creature.OnThink -= _creatureOnThinkEventHandler.Execute;

        if (creature is ICombatActor actor)
        {
            actor.OnDeath -= _creatureOnDeathEventHandler.Execute;
            actor.OnKill -= _creatureOnKillEventHandler.Execute;
            actor.OnBeforeDeath -= _creatureOnPrepareDeathEventHandler.Execute;
            actor.OnHealthChanged -= _creatureOnHealthChangeEventHandler.Execute;
            actor.OnManaChanged -= _creatureOnManaChangeEventHandler.Execute;
        }

        if (creature is IPlayer player)
        {
            player.OnLoggedIn -= _playerOnLoginEventHandler.Execute;
            player.OnLoggedOut -= _playerOnLogoutEventHandler.Execute;
            player.OnLevelAdvanced -= _playerOnAdvanceEventHandler.Execute;
            player.OnWroteText -= _playerOnTextEditEventHandler.Execute;
        }
    }
}