using NeoServer.Domain.Chat;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Services;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Location;

namespace NeoServer.Domain.Services;

public class CreatureSpeechService(IMap map) : ICreatureSpeechService
{
    public void Speak(ICreature sender, string message, SpeechType talkType)
    {
        if (sender is null) return;
        if (string.IsNullOrWhiteSpace(message) || talkType == SpeechType.None) return;
        
        var (maxDistanceX, maxDistanceY) = talkType switch
        {
            SpeechType.Yell or SpeechType.MonsterYell => ((int)MapViewPort.MaxClientViewPortX * 2 + 2,
                (int)MapViewPort.MaxClientViewPortY * 2 + 2),
            SpeechType.Whisper => (1, 1),
            _ => ((int)MapViewPort.MaxClientViewPortX, (int)MapViewPort.MaxClientViewPortY)
        };

        var multiFloor = talkType is SpeechType.Yell or SpeechType.MonsterYell;

        var spectators = map.GetSpectators(sender.Location, multiFloor, false, maxDistanceX, maxDistanceX,
            maxDistanceY, maxDistanceY);
        
        sender.Say(message, talkType, spectators.ToList());
    }

    public List<ICreature> GetYellSpectators(ICreature sender)
    {
        var (maxDistanceX, maxDistanceY) = ((int)MapViewPort.MaxClientViewPortX * 2 + 2,
            (int)MapViewPort.MaxClientViewPortY * 2 + 2);

       return map.GetSpectators(sender.Location, true, false, maxDistanceX, maxDistanceX,
            maxDistanceY, maxDistanceY).ToList();
    }
}
