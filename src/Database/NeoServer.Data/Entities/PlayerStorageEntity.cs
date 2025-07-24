namespace NeoServer.Data.Entities;

public class PlayerStorageEntity
{
    public int PlayerId { get; set; }
    public uint Key { get; set; }
    public int Value { get; set; }

    public virtual PlayerEntity Player { get; set; }
}