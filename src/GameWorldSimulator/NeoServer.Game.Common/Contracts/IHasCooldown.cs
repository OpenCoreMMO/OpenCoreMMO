using System;

namespace NeoServer.Game.Common.Contracts;

public interface IHasCooldown
{
    public Guid CooldownId { get; }
    public (int Id, uint Cooldown) PrimaryGroup { get;  }
    public (int Id, uint Cooldown) SecondaryGroup { get;  }
    public uint Cooldown { get; }
    public bool HasAnyCooldownGroup => PrimaryGroup.Id != 0 || SecondaryGroup.Id != 0;
    public bool HasCooldownGroup(int id)
    {
        return PrimaryGroup.Id == id || SecondaryGroup.Id == id;
    }
}