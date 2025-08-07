using System.Linq;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Networking.Packets.Incoming.Player;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Network;
using Serilog;

namespace NeoServer.Networking.Handlers.Player;

public class PlayerChangeCompletedOutFitHandler : PacketHandler
{
    private readonly IGameServer _game;
    private readonly IPlayerOutFitStore _playerOutFitStore;
    private readonly ILogger _logger;

    public PlayerChangeCompletedOutFitHandler(IGameServer game, IPlayerOutFitStore playerOutFitStore, ILogger logger)
    {
        _game = game;
        _playerOutFitStore = playerOutFitStore;
        _logger = logger;
    }

    public override void HandleMessage(IReadOnlyNetworkMessage message, IConnection connection)
    {
        if (!_game.CreatureManager.TryGetPlayer(connection.CreatureId, out var player)) return;

        var packet = new PlayerChangeOutFitPacket(message);
        
        _logger.Information("Player {PlayerName} trying to change outfit to: LookType={LookType}, Head={Head}, Body={Body}, Legs={Legs}, Feet={Feet}, Addon={Addon}, Mount={Mount}", 
            player.Name, packet.Outfit.LookType, packet.Outfit.Head, packet.Outfit.Body, packet.Outfit.Legs, packet.Outfit.Feet, packet.Outfit.Addon, packet.Mount);

        var playerOutfits = _playerOutFitStore.Get(player.Gender);
        if (playerOutfits == null)
        {
            _logger.Error("No outfits found in store for gender {Gender}. Player {PlayerName} cannot change outfit.", 
                player.Gender, player.Name);
            return;
        }

        var outfitToChange = playerOutfits.FirstOrDefault(item => item.LookType == packet.Outfit.LookType);

        if (outfitToChange is null) 
        {
            _logger.Warning("Player {PlayerName} tried to change to outfit {LookType} but it was not found in store for gender {Gender}", 
                player.Name, packet.Outfit.LookType, player.Gender);
            return;
        }

        var outfit = packet.Outfit
            .SetEnabled(outfitToChange.Enabled)
            .SetGender(outfitToChange.Type)
            .SetName(outfitToChange.Name)
            .SetPremium(outfitToChange.RequiresPremium)
            .SetUnlocked(outfitToChange.Unlocked);

        _logger.Information("Player {PlayerName} changing outfit: {OutfitName} (LookType={LookType}, Premium={Premium}, Unlocked={Unlocked})", 
            player.Name, outfit.Name, outfit.LookType, outfit.Premium, outfit.Unlocked);

        player.ChangeOutfit(outfit);
        
        _logger.Information("Player {PlayerName} outfit changed successfully", player.Name);
    }
}