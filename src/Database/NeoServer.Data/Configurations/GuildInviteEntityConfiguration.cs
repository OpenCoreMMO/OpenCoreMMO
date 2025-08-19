using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeoServer.Data.Entities;

namespace NeoServer.Data.Configurations;

public class GuildInviteEntityConfiguration : IEntityTypeConfiguration<GuildInviteEntity>
{
    public void Configure(EntityTypeBuilder<GuildInviteEntity> builder)
    {
        builder.HasKey(e => new { e.PlayerId, e.GuildId });
        
        builder.Property(e => e.PlayerId)
            .IsRequired();
            
        builder.Property(e => e.GuildId)
            .IsRequired();

        builder.HasOne(e => e.Player)
            .WithMany()
            .HasForeignKey(e => e.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(e => e.Guild)
            .WithMany()
            .HasForeignKey(e => e.GuildId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
