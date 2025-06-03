using System;
using System.Collections.Generic;
using System.Linq;
using NeoServer.Game.Common.Chats;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Common.Contracts.World.Tiles;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Helpers;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Game.Creatures.Models.Bases;

namespace NeoServer.Game.Creatures.Npcs;

public class Npc : WalkableCreature, INpc
{
    public readonly Dictionary<string, Func<string, INpc, ISociableCreature, string>> KeywordReplacementMap;

    public Npc(INpcType type, IMapTool mapTool, ISpawnPoint spawnPoint, IOutfit outfit = null,
        uint healthPoints = 0) : base(type,
        mapTool, outfit, healthPoints)
    {
        Metadata = type;
        SpawnPoint = spawnPoint;

        Cooldowns.Start(CooldownType.Advertise, 10_000);
        Cooldowns.Start(CooldownType.WalkAround, 5_000);

        KeywordReplacementMap = new Dictionary<string, Func<string, INpc, ISociableCreature, string>>
        {
            ["|PLAYERNAME|"] = (_, _, player) => player.Name
        };
    }

    public CreateItem CreateNewItem { protected get; init; }

    public override ITileEnterRule TileEnterRule => NpcEnterTileRule.Rule;
    public KeywordReplacement ReplaceKeywords { get; set; }

    public ISpawnPoint SpawnPoint { get; }
    public override IOutfit Outfit { get; protected set; }
    public INpcType Metadata { get; }

    public override bool CanSeeInvisible => false;

    public override bool CanBeSeen => true;

    public void Advertise()
    {
        if (!Metadata.Marketings?.Any() ?? true) return;

        if (!Cooldowns.Cooldowns[CooldownType.Advertise].Expired) return;
        Say(GameRandom.Random.Next(Metadata.Marketings), SpeechType.Say);
        Cooldowns.Start(CooldownType.Advertise, 10_000);
    }

    public override bool WalkRandomStep()
    {
        if (!Cooldowns.Cooldowns[CooldownType.WalkAround].Expired || SpawnPoint == null) return false;

        var result = base.WalkRandomStep(SpawnPoint.Location);

        Cooldowns.Start(CooldownType.WalkAround, Metadata.WalkInterval);
        return result;
    }

    public override bool CanSee(Location pos)
    {
        return base.CanSee(pos, 3, 3);
    }

    public void Hear(ICreature from, SpeechType speechType, string message)
    {
        if (from is null || speechType == SpeechType.None || string.IsNullOrWhiteSpace(message)) return;
        OnHear?.Invoke(from, this, speechType, message);
    }

    public void PlayerCloseChannel(IPlayer player)
    {
        if (player is null) return;
        OnPlayerCloseChannel?.Invoke(this, player);
    }

    public bool CanInteract(Location location, int range)
    {
        return Location.Z == location.Z && base.CanSee(location, range, range);
    }

    private IList<IPlayer> _playerInteractionsOrder = new List<IPlayer>();
    private IDictionary<IPlayer, ushort> _playerInteractions = new Dictionary<IPlayer, ushort>();

    public void SetPlayerInteraction(IPlayer player, ushort topicId)
    {
        if (!_playerInteractionsOrder.Contains(player))
        {
            _playerInteractionsOrder.Add(player);
            TurnTo(player);
        }

        _playerInteractions.AddOrUpdate(player, topicId);
    }

    public void RemovePlayerInteraction(IPlayer player)
    {
        _playerInteractionsOrder.Remove(player);

        if (_playerInteractions.ContainsKey(player))
        {
            _playerInteractions.Remove(player);

            if (this is IShopperNpc shopperNpc)
                shopperNpc.StopSellingToCustomer(player);
        }

        if (_playerInteractionsOrder.Count > 0)
            TurnTo(_playerInteractionsOrder.FirstOrDefault());
    }

    public bool IsInteractingWithAnyPlayer()
        => _playerInteractions.Count > 0;

    public bool IsInteractingWithPlayer(IPlayer player)
    {
        if (_playerInteractions.Count == 0)
            return false;

        return _playerInteractions.ContainsKey(player);
    }

    public bool IsPlayerInteractingOnTopic(IPlayer player, ushort topicId)
    {
        if (_playerInteractions.Count == 0)
            return false;

        if (!_playerInteractions.TryGetValue(player, out var currentTopicId))
            return false;

        return currentTopicId == topicId;
    }

    #region Events

    public event Hear OnHear;
    public event PlayerCloseChannel OnPlayerCloseChannel;
    public event CustomerLeft OnCustomerLeft;

    #endregion
}