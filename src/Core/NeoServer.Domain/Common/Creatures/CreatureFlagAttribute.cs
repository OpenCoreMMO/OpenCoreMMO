namespace NeoServer.Domain.Common.Creatures;

public enum CreatureFlagAttribute : byte
{
    Summonable,
    Attackable,
    Hostile,
    Illusionable,
    Convinceable,
    Pushable,
    CanPushItems,
    CanPushCreatures,
    TargetDistance,
    StaticAttack,
    RunOnHealth,
    None,
    LightColor,
    IsBoss,
    RewardBoss
}