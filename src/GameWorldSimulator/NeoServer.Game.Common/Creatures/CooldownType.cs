namespace NeoServer.Game.Common.Creatures;

public enum CooldownType
{
    Move,
    UpdatePath,
    Action,
    Combat,
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