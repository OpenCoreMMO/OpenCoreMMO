using System.Collections.Generic;
using System.Linq;
using NeoServer.Data.Entities;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Player;

public class PlayerOutFitWindowPacket : OutgoingPacket
{
    private readonly IEnumerable<IPlayerOutFit> _outfits;
    private readonly List<PlayerOutfitAddonEntity> _playerOutfitAddonModels;
    private readonly IPlayer player;

    public PlayerOutFitWindowPacket(IPlayer player, IEnumerable<IPlayerOutFit> outfits,
        List<PlayerOutfitAddonEntity> playerOutfitAddonModels)
    {
        this.player = player;
        _outfits = outfits;
        _playerOutfitAddonModels = playerOutfitAddonModels;
    }

    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte((byte)GameOutgoingPacketType.OutfitWindow);
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
        
        // Since mount is 0, we need to send mount colors anyway for protocol compatibility
        message.AddByte(0); // mount head color
        message.AddByte(0); // mount body color  
        message.AddByte(0); // mount legs color
        message.AddByte(0); // mount feet color
        
        // Add current familiar looktype (0 = no familiar for now)
        message.AddUInt16(0);

        var outfits = _outfits.Where(x => (!x.RequiresPremium || (player.PremiumTime > 0 && x.RequiresPremium)) &&
                                          x.Enabled).ToList();

        message.AddUInt16((ushort)outfits.Count);

        var playerAddons = GetPlayerAddonsMap();

        foreach (var outfit in outfits)
        {
            if (player.PremiumTime <= 0 && outfit.RequiresPremium) continue;

            playerAddons.TryGetValue(outfit.LookType, out var addonLevel);

            message.AddUInt16(outfit.LookType);
            message.AddString(outfit.Name);
            message.AddByte((byte)addonLevel); // Enable fully Addon to outfit.
        }

        // Add mounts list (empty for now)
        message.AddUInt16(0); // mounts count
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