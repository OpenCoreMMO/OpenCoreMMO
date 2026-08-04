using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Location.Structs;

namespace NeoServer.Domain.Creatures.Events.Monster;

public record MonsterWasBornEvent(IMonster Monster, Location Location) : IEvent;