using NeoServer.Domain.Chat;
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;

namespace NeoServer.Domain.Creatures.Events;

public record CreatureSayEvent(ICreature Sender, SpeechType SpeechType, string Message, List<ICreature> Receivers = null) : IEvent;
