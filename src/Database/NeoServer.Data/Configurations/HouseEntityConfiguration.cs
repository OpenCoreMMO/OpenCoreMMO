using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeoServer.Data.Entities;

namespace NeoServer.Data.Configurations;

public class HouseEntityConfiguration : IEntityTypeConfiguration<HouseEntity>
{
    public void Configure(EntityTypeBuilder<HouseEntity> builder)
    {
        builder.HasKey(h => h.Id);
        
        builder.Property(h => h.Id)
            .ValueGeneratedOnAdd()
            .IsRequired();
            
        builder.Property(h => h.Name)
            .HasMaxLength(255)
            .IsRequired();
            
        builder.Property(h => h.Owner)
            .HasDefaultValue(0)
            .IsRequired();
            
        builder.Property(h => h.TownId)
            .IsRequired();
            
        builder.Property(h => h.Price)
            .HasDefaultValue(0)
            .IsRequired();
            
        builder.Property(h => h.Rent)
            .HasDefaultValue(0)
            .IsRequired();
            
        builder.Property(h => h.Size)
            .HasDefaultValue(0)
            .IsRequired();
            
        builder.Property(h => h.PaidUntil)
            .HasDefaultValue(0)
            .IsRequired();
            
        builder.Property(h => h.LastPayment)
            .IsRequired(false);
            
        builder.Property(h => h.CreatedDate)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();
            
        builder.Property(h => h.ModifiedDate)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();
            
        builder.Property(h => h.EntryCoordinates)
            .HasColumnType("TEXT");
            
        builder.Property(h => h.TileCoordinates)
            .HasColumnType("TEXT");
            
        builder.Property(h => h.DoorCoordinates)
            .HasColumnType("TEXT");

        builder.HasIndex(h => h.Owner);
        builder.HasIndex(h => h.TownId);
        builder.HasIndex(h => h.Name);
        
        builder.HasMany(h => h.HouseLists)
            .WithOne(hl => hl.House)
            .HasForeignKey(hl => hl.HouseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}