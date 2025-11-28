namespace NeoServer.Domain.Guild;

public class GuildRankInfo
{
    public GuildRankInfo()
    {
    }

    public GuildRankInfo(ushort id, string name, byte level)
    {
        Id = id;
        Name = name;
        Level = level;
    }

    public ushort Id { get; set; }
    public string Name { get; set; }
    public byte Level { get; set; }

    public override bool Equals(object obj)
    {
        if (obj is GuildRankInfo other) return Id == other.Id;
        return false;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public override string ToString()
    {
        return $"{Name} (Level {Level})";
    }
}