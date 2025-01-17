using System.Collections.Immutable;
using NeoServer.Game.Common.Contracts.Services;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Common.Creatures;

namespace NeoServer.Game.Common.Contracts.Creatures;

public delegate void Born(IMonster monster, Location.Structs.Location location);

public delegate void MonsterChangeState(IMonster monster, MonsterState fromState, MonsterState toState);

public interface IMonster : IWalkableMonster, ICombatActor
{
    /// <summary>
    ///     Monster metadata
    /// </summary>
    IMonsterType Metadata { get; }

    /// <summary>
    ///     Monster spawn location
    /// </summary>
    public ISpawnPoint Spawn { get; }

    /// <summary>
    ///     Indicates whether monster born from spawn
    /// </summary>
    public bool BornFromSpawn => Spawn != null;

    /// <summary>
    ///     Monster state
    /// </summary>
    MonsterState State { get; }

    /// <summary>
    ///     Experience that monster can give
    /// </summary>
    uint Experience { get; }

    /// <summary>
    ///     Indicates that monster is in combat
    /// </summary>
    bool IsInCombat { get; }

    bool Defending { get; }

    /// <summary>
    ///     All damages that monster received since has born
    /// </summary>
    ImmutableDictionary<ICreature, ushort> Damages { get; }

    /// <summary>
    ///     Indicates if monster is sleeping
    /// </summary>
    bool IsSleeping { get; }
    bool IsSummon { get; }
    bool IsHostile { get; }
    bool IsCurrentTargetUnreachable { get; }
    event Born OnWasBorn;
    event MonsterChangeState OnChangedState;

    void Reborn();

    /// <summary>
    ///     Select a target to attack
    /// </summary>
    void SelectTargetToAttack();

    /// <summary>
    ///     Executes defense action
    /// </summary>
    /// <returns>interval</returns>
    ushort Defend();

    void MoveAroundEnemy();
    void Sleep();

    /// <summary>
    ///     Monster yells a sentence
    /// </summary>
    void Yell();

    /// <summary>
    ///     Changes monster's state based on targets and condition
    /// </summary>
    void UpdateState();

    void Escape();
    void Born(Location.Structs.Location location);
    void Summon(ISummonService summonService);
}