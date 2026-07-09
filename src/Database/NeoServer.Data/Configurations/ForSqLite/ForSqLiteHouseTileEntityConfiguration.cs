using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeoServer.Data.Entities;

namespace NeoServer.Data.Configurations.ForSqLite;

public class ForSqLiteHouseTileEntityConfiguration : IEntityTypeConfiguration<HouseTileEntity>
{
    public void Configure(EntityTypeBuilder<HouseTileEntity> builder)
    {
        builder.HasKey(ht => new { ht.HouseId, ht.TileX, ht.TileY, ht.TileZ });
        builder.Property(ht => ht.TileX).IsRequired();
        builder.Property(ht => ht.TileY).IsRequired();
        builder.Property(ht => ht.TileZ).IsRequired();
        builder.Property(ht => ht.Data).HasColumnType("BLOB");
        builder.HasOne(ht => ht.House)
            .WithMany(h => h.HouseTiles)
            .HasForeignKey(ht => ht.HouseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
