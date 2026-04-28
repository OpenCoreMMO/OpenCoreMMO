using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Creatures;

namespace NeoServer.Domain.Creatures.Events.Player;

public record PlayerLevelAdvancedEvent(IPlayer Player, SkillType Type, int FromLevel, int ToLevel) : IEvent;
