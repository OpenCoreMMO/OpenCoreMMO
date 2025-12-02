using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Services;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Creatures;
using NeoServer.Domain.Creatures.Monster;
using NeoServer.Domain.Creatures.Monster.Combat;

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
    ///     Indicates if monster is sleeping
    /// </summary>
    bool IsSleeping { get; }

    bool IsSummon { get; }
    bool IsHostile { get; }
    MonsterTargetList Targets { get; set; }
    bool IsPushable { get; }

    void Reborn();

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
    void Born(Location location);
    void CreateSummon(ISummonService summonService);
    void PostAttack(MonsterCombatType type);
    bool IsImmune(Immunity immunity);
    bool IsImmune(DamageType damageType);
}