using NeoServer.Domain.Common.Contracts.Creatures;

namespace NeoServer.Domain.Creatures.Experience;

public interface IBaseExperienceModifier
{
    string Name { get; }

    /// <summary>
    ///     Modifies the base experience to be a new value.
    /// </summary>
    /// <param name="player">The player gaining the experience.</param>
    /// <param name="monster">The monster being killed.</param>
    /// <param name="baseExperience">The input base experience.</param>
    /// <returns>The output base experience.</returns>
    uint GetModifiedBaseExperience(IPlayer player, IMonster monster, uint baseExperience);

    /// <summary>
    ///     Does the experience bonus apply to this situation?
    /// </summary>
    /// <param name="player">Player gaining experience.</param>
    /// <param name="monster">Monster that was killed.</param>
    bool IsEnabled(IPlayer player, IMonster monster);
}