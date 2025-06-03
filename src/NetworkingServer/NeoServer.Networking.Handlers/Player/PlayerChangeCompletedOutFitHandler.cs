using System.Linq;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Networking.Packets.Incoming.Player;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Handlers.Player;

public class PlayerChangeCompletedOutFitHandler : PacketHandler
{
    private readonly IGameServer _game;
    private readonly IPlayerOutFitStore _playerOutFitStore;

    public PlayerChangeCompletedOutFitHandler(IGameServer game, IPlayerOutFitStore playerOutFitStore)
    {
        _game = game;
        _playerOutFitStore = playerOutFitStore;
    }

    public override void HandleMessage(IReadOnlyNetworkMessage message, IConnection connection)
    {
        if (!_game.CreatureManager.TryGetPlayer(connection.CreatureId, out var player)) return;

        var packet = new PlayerChangeOutFitPacket(message);

        var outfitToChange = _playerOutFitStore.Get(player.Gender)
            .FirstOrDefault(item => item.LookType == packet.Outfit.LookType);

        if (outfitToChange is null) return;

        var outfit = packet.Outfit
            .SetEnabled(outfitToChange.Enabled)
            .SetGender(outfitToChange.Type)
            .SetName(outfitToChange.Name)
            .SetPremium(outfitToChange.RequiresPremium)
            .SetUnlocked(outfitToChange.Unlocked);

        player.ChangeOutfit(outfit);
    }
}