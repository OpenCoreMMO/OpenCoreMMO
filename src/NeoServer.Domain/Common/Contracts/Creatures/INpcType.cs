using NeoServer.Domain.Common.Creatures;

namespace NeoServer.Domain.Common.Contracts.Creatures;

public interface INpcType : ICreatureType
{
    public string Script { get; set; }
    public IDictionary<string, dynamic> CustomAttributes { get; }
    bool IsLuaScript { get; }
    string[] Marketings { init; get; }
    uint WalkInterval { get; set; }
    uint WalkRadius { get; set; }
    bool IsCSharpScript { get; }

    IList<Voice> Voices { get; }

    IIntervalChance VoiceConfig { get; set; }
    IDictionary<ushort, IShopItem> ShopItems { get; }
}