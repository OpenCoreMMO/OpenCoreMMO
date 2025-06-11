namespace NeoServer.Domain.Common.Creatures;

public enum CooldownType
{
    None = 0,
    Move,
    UpdatePath,
    Action,
    WeaponAttack,
    Talk,
    Block,
    LookForNewEnemy,
    Haste,
    Spell,
    MoveAroundEnemy,
    TargetChange,
    Yell,
    HealthRecovery,
    ManaRecovery,
    SoulRecovery,

    /// <summary>
    ///     time that monster can be awaken without any target around
    /// </summary>
    Awaken,
    Advertise,
    WalkAround,
    UseItem,
    SupportSpell
}