using NeoServer.Domain.Chat;
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Creatures.Events;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Creatures.Models.Bases;
using NeoServer.Domain.Creatures.Player.Outfit;

namespace NeoServer.Domain.Creatures.Npcs;

public class Npc : WalkableCreature, INpc
{
    private readonly IDictionary<IPlayer, ushort> _playerInteractions = new Dictionary<IPlayer, ushort>();

    private readonly IList<IPlayer> _playerInteractionsOrder = new List<IPlayer>();
    public readonly Dictionary<string, Func<string, INpc, ISociableCreature, string>> KeywordReplacementMap;

    public Npc(INpcType type, IMapTool mapTool, ISpawnPoint spawnPoint, Outfit outfit = null,
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
    public override Outfit Outfit { get; protected set; }
    public INpcType Metadata { get; }

    public override bool CanSeeInvisible => false;

    /// <summary>
    /// Allows the NPC to advertise its marketing messages to a list of sociable creatures (receivers), if applicable.
    /// </summary>
    /// <param name="receivers">The list of creatures that will receive the advertisement message.</param>
    /// <remarks>
    /// The method ensures that the NPC has marketing messages available and checks if the advertisement
    /// cooldown has expired before proceeding. It then selects a random marketing message and broadcasts
    /// it to the specified receivers, also resetting the cooldown timer for advertising.
    /// </remarks>
    public void Advertise(List<ICreature> receivers)
    {
        if ((Metadata.Marketings?.Length ?? 0) <= 0) return;

        if (!Cooldowns.Cooldowns[CooldownType.Advertise].Expired) return;
        
        Say(GameRandom.Random.Next(Metadata.Marketings), SpeechType.Say, receivers);
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
        EventAggregator.Invoke(new CreatureHearEvent(from, this, speechType, message));
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

        if (_playerInteractions.Remove(player))
            if (this is IShopperNpc shopperNpc)
                shopperNpc.StopSellingToCustomer(player);

        if (_playerInteractionsOrder.Count > 0)
            TurnTo(_playerInteractionsOrder.FirstOrDefault());
    }

    public bool IsInteractingWithAnyPlayer()
    {
        return _playerInteractions.Count > 0;
    }

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

    public event PlayerCloseChannel OnPlayerCloseChannel;

    #endregion
}