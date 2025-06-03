using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeoServer.Data.Entities;

namespace NeoServer.Data.Configurations;

public class HouseListEntityConfiguration : IEntityTypeConfiguration<HouseListEntity>
{
    public void Configure(EntityTypeBuilder<HouseListEntity> builder)
    {
        builder.HasKey(hl => new { hl.HouseId, hl.ListId });
        builder.Property(hl => hl.List).IsRequired();
        builder.HasOne(hl => hl.House)
            .WithMany(h => h.HouseLists)
            .HasForeignKey(hl => hl.HouseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}