namespace NeoServer.Domain.Common.Contracts;

public interface IHasCooldown
{
    public Guid CooldownId { get; }
    public (string Name, uint Cooldown) PrimaryGroup { get; set; }
    public (string Name, uint Cooldown) SecondaryGroup { get; set; }
    public uint Cooldown { get; set; }

    public bool HasAnyCooldownGroup =>
        !string.IsNullOrWhiteSpace(PrimaryGroup.Name) || !string.IsNullOrWhiteSpace(SecondaryGroup.Name);

    public bool HasCooldownGroup(string name)
    {
        return string.Equals(PrimaryGroup.Name, name, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(SecondaryGroup.Name, name, StringComparison.OrdinalIgnoreCase);
    }
}
