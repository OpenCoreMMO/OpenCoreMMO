using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Creatures.Player.Modes;

namespace NeoServer.Domain.Creatures.Events.Player;

public record PlayerChangedChaseModeEvent(IPlayer Player, ChaseMode OldChaseMode, ChaseMode NewChaseMode) : IEvent;
