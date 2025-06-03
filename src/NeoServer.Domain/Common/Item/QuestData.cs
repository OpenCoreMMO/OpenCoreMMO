namespace NeoServer.Domain.Common.Item;

public class QuestData
{
    public (ushort, uint) Key => (ActionId, UniqueId);
    public ushort ActionId { get; set; }
    public uint UniqueId { get; set; }
    public string Script { get; set; }
    public Reward[] Rewards { get; set; }
    public string Name { get; set; }
    public string Group { get; set; }
    public string GroupKey { get; set; }
    public bool AutoLoad { get; set; }

    public class Reward
    {
        public ushort ItemId { get; set; }
        public byte Amount { get; set; }
        public Reward[] Children { get; set; }
    }
}