using NeoServer.Domain.Chat;
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;

namespace NeoServer.Domain.Creatures.Events.Player;

public record PlayerSentMessageEvent(ISociableCreature From, ISociableCreature To, SpeechType SpeechType, string Message) : IEvent;
