using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Creatures.Monster;

namespace NeoServer.Domain.Creatures.Events.Monster;

public record MonsterStateChangedEvent(IMonster Monster, MonsterState FromState, MonsterState ToState) : IEvent;