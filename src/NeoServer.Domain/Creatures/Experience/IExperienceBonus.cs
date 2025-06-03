using NeoServer.Domain.Common.Contracts.Creatures;

namespace NeoServer.Domain.Creatures.Experience;

/// <summary>
///     A standardized way of defining experience bonuses.
/// </summary>
public interface IExperienceBonus
{
    ExperienceBonusType BonusType { get; }
    string Name { get; }

    /// <summary>
    ///     The bonus factor amount (e.g. if it is a 50% bonus this would return 0.5).
    /// </summary>
    /// <param name="player">Player gaining experience.</param>
    /// <param name="monster">Monster that was killed.</param>
    double GetBonusFactorAmount(IPlayer player, IMonster monster);

    /// <summary>
    ///     Does the experience bonus apply to this situation?
    /// </summary>
    /// <param name="player">Player gaining experience.</param>
    /// <param name="monster">Monster that was killed.</param>
    bool IsEnabled(IPlayer player, IMonster monster);
}