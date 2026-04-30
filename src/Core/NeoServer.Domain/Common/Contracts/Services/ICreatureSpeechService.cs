using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Chat;

namespace NeoServer.Domain.Common.Contracts.Services;

/// <summary>
/// Provides functionalities for handling creature speech actions, including
/// broadcasting messages to appropriate spectators based on speech type and range.
/// </summary>
public interface ICreatureSpeechService
{
    /// <summary>
    /// Handles the speech action for a creature, broadcasting the message
    /// to all eligible spectators within the configured range based on the speech type.
    /// </summary>
    /// <param name="sender">The creature initiating the speech action. If null, the method does nothing.</param>
    /// <param name="message">The message to be spoken. If empty, null, or whitespace, the method does nothing.</param>
    /// <param name="talkType">
    /// The type of speech being used. Determines the range and behavior of the speech.
    /// For example, whisper, yell, or regular say. If set to <see cref="SpeechType.None"/>, the method does nothing.
    /// </param>
    void Speak(ICreature sender, string message, SpeechType talkType);

    List<ICreature> GetYellSpectators(ICreature sender);
}
