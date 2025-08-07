using System.Collections.Generic;
using System.Linq;
using NeoServer.Data.Entities;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Server.Common.Contracts.Network;
using Serilog;

namespace NeoServer.Networking.Packets.Outgoing.Player;

public class PlayerOutFitWindowPacket : OutgoingPacket
{
    private readonly IEnumerable<IPlayerOutFit> _outfits;
    private readonly List<PlayerOutfitAddonEntity> _playerOutfitAddonModels;
    private readonly IPlayer player;
    private readonly ILogger _logger;

    public PlayerOutFitWindowPacket(IPlayer player, IEnumerable<IPlayerOutFit> outfits,
        List<PlayerOutfitAddonEntity> playerOutfitAddonModels)
    {
        this.player = player;
        _outfits = outfits;
        _playerOutfitAddonModels = playerOutfitAddonModels;
        _logger = Log.ForContext<PlayerOutFitWindowPacket>();
    }

    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte((byte)GameOutgoingPacketType.OutfitWindow);
        
        // Add current outfit (like AddOutfit in forgottenserver)
        message.AddUInt16(player.Outfit.LookType);

        if (player.Outfit.LookType != 0)
        {
            message.AddByte(player.Outfit.Head);
            message.AddByte(player.Outfit.Body);
            message.AddByte(player.Outfit.Legs);
            message.AddByte(player.Outfit.Feet);
            message.AddByte(player.Outfit.Addon);
        }
        else
        {
            message.AddUInt16(0); // lookTypeEx for non-player creatures
        }

        // Add current mount ID (0 = no mount for now)
        message.AddUInt16(0);

        var outfits = (_outfits ?? Enumerable.Empty<IPlayerOutFit>())
            .Where(x => (!x.RequiresPremium || (player.PremiumTime > 0 && x.RequiresPremium)) && x.Enabled)
            .ToList();

        _logger.Information("Sending outfit window to {PlayerName}: {OutfitCount} outfits available, {TotalOutfits} total outfits", 
            player.Name, outfits.Count, _outfits?.Count() ?? 0);

        message.AddByte((byte)outfits.Count);

        var playerAddons = GetPlayerAddonsMap();

        foreach (var outfit in outfits)
        {
            if (player.PremiumTime <= 0 && outfit.RequiresPremium) continue;

            playerAddons.TryGetValue(outfit.LookType, out var addonLevel);

            _logger.Debug("Adding outfit to packet: LookType={LookType}, Name={Name}, Addons={Addons}", 
                outfit.LookType, outfit.Name, addonLevel);

            message.AddUInt16(outfit.LookType);
            message.AddString(outfit.Name);
            message.AddByte((byte)addonLevel); // Enable fully Addon to outfit.
        }

        // Add mounts list (empty for now)
        message.AddByte(0); // mounts count
    }

    private Dictionary<int, int> GetPlayerAddonsMap()
    {
        var playerAddons = new Dictionary<int, int>();

        foreach (var playerOutfit in _playerOutfitAddonModels)
        {
            if (!playerAddons.ContainsKey(playerOutfit.LookType))
            {
                playerAddons[playerOutfit.LookType] = (byte)playerOutfit.AddonLevel;
                continue;
            }

            var addon = playerAddons[playerOutfit.LookType];
            playerAddons[playerOutfit.LookType] = addon | (byte)playerOutfit.AddonLevel;
        }

        return playerAddons;
    }
}