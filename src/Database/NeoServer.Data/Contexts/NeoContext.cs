using Microsoft.EntityFrameworkCore;
using NeoServer.Data.Configurations;
using NeoServer.Data.Configurations.ForSqLite;
using NeoServer.Data.Entities;
using Serilog;
using System.Data.Common;

namespace NeoServer.Data.Contexts;

public class NeoContext : DbContext
{
    private readonly ILogger _logger;

    public NeoContext(DbContextOptions<NeoContext> options, ILogger logger)
        : base(options)
    {
        _logger = logger;
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
    public DbSet<PlayerStorageEntity> PlayerStorages { get; set; }
    public DbSet<WorldRecordEntity> WorldRecords { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.LogTo(m => _logger.Verbose(m),
            (eventId, _) => eventId.Name == $"{DbLoggerCategory.Database.Command.Name}.CommandExecuted");
    }

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
        modelBuilder.ApplyConfiguration(new PlayerStorageEntityConfiguration());

        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            entity.SetTableName(entity.GetTableName().ToLower());

            foreach (var property in entity.GetProperties())
                property.SetColumnName(property.Name.ToLower());

            foreach (var key in entity.GetKeys())
                key.SetName(key.GetName().ToLower());

            foreach (var fk in entity.GetForeignKeys())
                fk.SetConstraintName(fk.GetConstraintName().ToLower());
        }

        base.OnModelCreating(modelBuilder);
    }

    public bool ExistsTable(string tableName)
    {
        var sql = @"
        SELECT EXISTS (
            SELECT 1 
            FROM information_schema.tables 
            WHERE table_schema = 'public' 
            AND table_name = {0}
        )";

        return Database.ExecuteSqlRaw(sql, tableName) == 1;
    }

    public DbCommand CreateDbCommand(string query)
    {
        var command = Database.GetDbConnection().CreateCommand();
        command.CommandText = query;
        command.CommandType = System.Data.CommandType.Text;

        Database.OpenConnection();

        return command;
    }
}