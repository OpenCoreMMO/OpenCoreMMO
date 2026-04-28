using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Creatures;

namespace NeoServer.Domain.Creatures.Events.Player;

public record PlayerGainedSkillPointEvent(IPlayer Player, SkillType Type) : IEvent;
