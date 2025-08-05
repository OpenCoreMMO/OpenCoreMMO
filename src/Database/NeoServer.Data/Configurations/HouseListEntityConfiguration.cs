using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeoServer.Data.Entities;

namespace NeoServer.Data.Configurations;

public class HouseListEntityConfiguration : IEntityTypeConfiguration<HouseListEntity>
{
    public void Configure(EntityTypeBuilder<HouseListEntity> builder)
    {
        builder.HasKey(hl => new { hl.HouseId, hl.PlayerId });
        
        builder.Property(hl => hl.HouseId)
            .IsRequired();
            
        builder.Property(hl => hl.PlayerId)
            .IsRequired();
            
        builder.Property(hl => hl.ListType)
            .IsRequired()
            .HasComment("1=Guest, 2=SubOwner");
            
        builder.Property(hl => hl.CreatedDate)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();
            
        builder.HasOne(hl => hl.House)
            .WithMany(h => h.HouseLists)
            .HasForeignKey(hl => hl.HouseId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasIndex(hl => hl.PlayerId);
        builder.HasIndex(hl => new { hl.HouseId, hl.ListType });
    }
}