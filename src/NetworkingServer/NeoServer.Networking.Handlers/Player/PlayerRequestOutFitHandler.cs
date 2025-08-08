using System.Linq;
using NeoServer.Data.Interfaces;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Networking.Packets.Outgoing.Player;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Network;
using Serilog;

namespace NeoServer.Networking.Handlers.Player;

public class PlayerRequestOutFitHandler : PacketHandler
{
    private readonly IGameServer _game;
    private readonly IPlayerOutFitStore _playerOutFitStore;
    private readonly IPlayerRepository _playerRepository;
    private readonly ILogger _logger;

    public PlayerRequestOutFitHandler(IGameServer game, IPlayerOutFitStore playerOutFitStore,
        IPlayerRepository playerRepository, ILogger logger)
    {
        _game = game;
        _playerOutFitStore = playerOutFitStore;
        _playerRepository = playerRepository;
        _logger = logger;
    }

    public override void HandleMessage(IReadOnlyNetworkMessage message, IConnection connection)
    {
        if (!_game.CreatureManager.TryGetPlayer(connection.CreatureId, out var player)) return;

        if (player.IsNull()) return;

        _logger.Information("Player {PlayerName} (Gender: {Gender}) requesting outfit window", player.Name, player.Gender);
        
        var outfits = _playerOutFitStore.Get(player.Gender);
        
        if (outfits == null)
        {
            _logger.Warning("No outfits found for player {PlayerName} with gender {Gender}", player.Name, player.Gender);
            outfits = Enumerable.Empty<IPlayerOutFit>();
        }
        else
        {
            _logger.Information("Found {OutfitCount} outfits for player {PlayerName} with gender {Gender}", 
                outfits.Count(), player.Name, player.Gender);
            
            foreach (var outfit in outfits.Take(5)) // Log first 5 outfits for debugging
            {
                _logger.Debug("Outfit: LookType={LookType}, Name={Name}, Premium={Premium}, Enabled={Enabled}", 
                    outfit.LookType, outfit.Name, outfit.RequiresPremium, outfit.Enabled);
            }
        }

        var playerAddons = _playerRepository.GetOutfitAddons((int)player.Id).Result;
        _logger.Information("Player {PlayerName} has {AddonCount} outfit addons", player.Name, playerAddons?.Count ?? 0);

        connection.OutgoingPackets.Enqueue(new PlayerOutFitWindowPacket(player, outfits, playerAddons));
        connection.Send();
    }
}