using NeoServer.Domain.Chat;

namespace NeoServer.Domain.Common.Contracts.Creatures;

public interface ISociableCreature : IWalkableCreature
{
    void Hear(ICreature from, SpeechType speechType, string message);
}