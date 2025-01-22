using System.Collections.Generic;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Creatures;

namespace NeoServer.Game.Creatures.Npcs;

public sealed class NpcType : INpcType
{
    public string Description { get; set; }
    public IDialog[] Dialogs { get; init; }

    public string Name { get; set; }

    public uint Health { get; set; }
    public uint MaxHealth { get; set; }

    public ushort Speed { get; set; }

    public IDictionary<LookType, ushort> Look { get; set; }
    public string Script { get; set; }

    public bool IsLuaScript => Script?.EndsWith(".lua") ?? false;
    public bool IsCSharpScript => Script?.EndsWith(".cs") ?? false;

    public IDictionary<string, dynamic> CustomAttributes { get; } = new Dictionary<string, dynamic>();
    public string[] Marketings { get; init; }
    public uint WalkInterval { get; set; }
    public uint WalkRadius { get; set; }

    public IList<Voice> Voices { get; } = new List<Voice>();

    public IIntervalChance VoiceConfig { get; set; }

    public IDictionary<ushort, IShopItem> ShopItems { get; } = new Dictionary<ushort, IShopItem>();
}

public sealed class Dialog : IDialog
{
    public string[] OnWords { get; init; }
    public string[] Answers { get; init; }
    public string Action { get; init; }

    /// <summary>
    ///     Indicated how many times to back in dialog
    /// </summary>
    public byte Back { get; init; }

    public string StoreAt { get; init; }

    public bool End { get; init; }
    public IDialog[] Then { get; init; }
}