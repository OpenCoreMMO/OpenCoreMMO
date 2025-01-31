using System.Collections.Generic;
using System.Linq;
using NeoServer.Data.Entities;
using NeoServer.Game.Common.Contracts.Creatures;
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

    public override byte PacketType => (byte)GameOutgoingPacketType.OutfitWindow;

    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte(PacketType);
        message.AddUInt16(player.Outfit.LookType);

        if (player.Outfit.LookType != 0)
        {
            message.AddByte(player.Outfit.Head);
            message.AddByte(player.Outfit.Body);
            message.AddByte(player.Outfit.Legs);
            message.AddByte(player.Outfit.Feet);
            message.AddByte(player.Outfit.Addon);
        }

        var outfits = _outfits.Where(x => (!x.RequiresPremium || (player.PremiumTime > 0 && x.RequiresPremium)) &&
                                          x.Enabled).ToList();

        message.AddByte((byte)outfits.Count);

        var playerAddons = GetPlayerAddonsMap();

        foreach (var outfit in outfits)
        {
            if (player.PremiumTime <= 0 && outfit.RequiresPremium) continue;

            playerAddons.TryGetValue(outfit.LookType, out var addonLevel);

            message.AddUInt16(outfit.LookType);
            message.AddString(outfit.Name);
            message.AddByte((byte)addonLevel); // Enable fully Addon to outfit.
        }
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