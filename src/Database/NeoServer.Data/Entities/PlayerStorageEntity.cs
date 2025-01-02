namespace NeoServer.Data.Entities;

public class PlayerStorageEntity
{
    public int PlayerId { get; set; }
    public int Key { get; set; }
    public int Value { get; set; }

    public virtual PlayerEntity Player { get; set; }
}