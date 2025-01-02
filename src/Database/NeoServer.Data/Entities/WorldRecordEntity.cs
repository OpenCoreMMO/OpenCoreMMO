using System;

namespace NeoServer.Data.Entities;

public class WorldRecordEntity
{
    public int Id { get; set; }
    public int WordId { get; set; }
    public int Record { get; set; }
    public DateTime CreatedAt { get; set; }

    public virtual WorldEntity World { get; set; }
}