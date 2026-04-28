using NeoServer.Domain.Chat;
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;

namespace NeoServer.Domain.Creatures.Events;

public record CreatureHearEvent(ICreature From, ISociableCreature Receiver, SpeechType SpeechType, string Message) : IEvent;
