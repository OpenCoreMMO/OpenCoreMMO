namespace NeoServer.Domain.Creatures.Monster;

/// <summary>
///     A state machine no control monster
/// </summary>
public enum MonsterState
{
    Sleeping,
    InCombat,
    Escaping,
    LookingForEnemy,
    Awake
}