using Microsoft.EntityFrameworkCore;
using NeoServer.Data.Configurations;
using NeoServer.Data.Configurations.ForSqLite;
using NeoServer.Data.Entities;
using Serilog;

namespace NeoServer.Data.Contexts;

public class NeoContext : DbContext
{
    //private readonly ILogger _logger;

    public NeoContext(DbContextOptions<NeoContext> options/*, ILogger logger*/)
        : base(options)
    {
        //_logger = logger;
    }

    public DbSet<AccountEntity> Accounts { get; set; }
    public DbSet<PlayerEntity> Players { get; set; }
    public DbSet<PlayerItemEntity> PlayerItems { get; set; }
    public DbSet<PlayerDepotItemEntity> PlayerDepotItems { get; set; }
    public DbSet<PlayerInventoryItemEntity> PlayerInventoryItems { get; set; }
    public DbSet<AccountVipListEntity> AccountsVipList { get; set; }
    public DbSet<GuildEntity> Guilds { get; set; }
    public DbSet<GuildMembershipEntity> GuildMemberships { get; set; }
    public DbSet<WorldEntity> Worlds { get; set; }
    public DbSet<PlayerQuestEntity> PlayerQuests { get; set; }
    public DbSet<PlayerOutfitAddonEntity> PlayerOutfitAddons { get; set; }

    //protected override void OnConfiguring(DbContextOptions optionsBuilder)
    //{
    //    optionsBuilder.UseInMemoryDatabase("fdf");
    //    //optionsBuilder.LogTo(m => _logger.Verbose(m),
    //    //    (eventId, _) => eventId.Name == $"{DbLoggerCategory.Database.Command.Name}.CommandExecuted");
    //}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        if (Database.IsSqlite())
        {
            modelBuilder.ApplyConfiguration(new ForSqLitePlayerInventoryItemEntityConfiguration());
            modelBuilder.ApplyConfiguration(new ForSqLitePlayerItemEntityConfiguration());
            modelBuilder.ApplyConfiguration(new ForSqLitePlayerDepotItemEntityConfiguration());
            modelBuilder.ApplyConfiguration(new ForSqLitePlayerEntityConfiguration());
            modelBuilder.ApplyConfiguration(new ForSqLiteAccountEntityConfiguration());
            modelBuilder.ApplyConfiguration(new ForSqLiteGuildEntityConfiguration());
            modelBuilder.ApplyConfiguration(new ForSqLiteGuildRankEntityConfiguration());
            modelBuilder.ApplyConfiguration(new ForSqLiteWorldEntityConfiguration());
        }
        else
        {
            modelBuilder.ApplyConfiguration(new AccountEntityConfiguration());
            modelBuilder.ApplyConfiguration(new PlayerInventoryItemEntityConfiguration());
            modelBuilder.ApplyConfiguration(new PlayerItemEntityConfiguration());
            modelBuilder.ApplyConfiguration(new PlayerDepotItemEntitytConfiguration());
            modelBuilder.ApplyConfiguration(new PlayerEntityConfiguration());
            modelBuilder.ApplyConfiguration(new GuildEntityConfiguration());
            modelBuilder.ApplyConfiguration(new GuildRankEntityConfiguration());
            modelBuilder.ApplyConfiguration(new WorldEntityConfiguration());
        }

        modelBuilder.ApplyConfiguration(new PlayerQuestEntityConfiguration());
        modelBuilder.ApplyConfiguration(new PlayerOutfitAddonEntityConfiguration());
        modelBuilder.ApplyConfiguration(new AccountVipListEntityConfiguration());
        modelBuilder.ApplyConfiguration(new GuildMembershipEntityConfiguration());

        base.OnModelCreating(modelBuilder);
    }
}