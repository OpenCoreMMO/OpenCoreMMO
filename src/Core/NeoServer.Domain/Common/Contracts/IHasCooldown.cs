namespace NeoServer.Domain.Common.Contracts;

public interface IHasCooldown
{
    public Guid CooldownId { get; }
    public (string Name, uint Cooldown) PrimaryGroup { get; set; }
    public (string Name, uint Cooldown) SecondaryGroup { get; set; }
    public uint Cooldown { get; set; }

    public bool HasAnyCooldownGroup =>
        !string.IsNullOrWhiteSpace(PrimaryGroup.Name) || !string.IsNullOrWhiteSpace(PrimaryGroup.Name);

    public bool HasCooldownGroup(string name)
    {
        return PrimaryGroup.Name == name || SecondaryGroup.Name == name;
    }
}