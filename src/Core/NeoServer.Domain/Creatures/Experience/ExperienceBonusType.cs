namespace NeoServer.Domain.Creatures.Experience;

/// <summary>
///     Bonuses of the same group are added together and then multiplied to the base (monster) experience to get the bonus
///     experience.
///     Bonuses of different groups are multiplied together.
///     Formula:
///     GE = ME * (1 + Standard) * (1 + Stamina) * (1 + GlobalBonus)
///     WHERE:
///     GE = Gained Expereince
///     ME = Monster Experience
/// </summary>
public enum ExperienceBonusType
{
    Standard, // e.g. Prey Bonus, Party Shared Experience Bonus, etc.
    Stamina,
    GlobalBonus
}