using System.Collections.Generic;

namespace NeoServer.Data.Entities;

public class WorldEntity
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Ip { get; set; }
    public int Port { get; set; }

    public ICollection<WorldRecordEntity> WorldRecords { get; set; }
}