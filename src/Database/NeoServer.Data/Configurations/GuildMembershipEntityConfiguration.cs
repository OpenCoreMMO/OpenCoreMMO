using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeoServer.Data.Entities;

namespace NeoServer.Data.Configurations;

public class GuildMembershipEntityConfiguration : IEntityTypeConfiguration<GuildMembershipEntity>
{
    public void Configure(EntityTypeBuilder<GuildMembershipEntity> builder)
    {
        builder.HasKey(e => new { e.PlayerId, e.GuildId });

        builder.Property(e => e.PlayerId);
        builder.Property(e => e.GuildId);
        builder.Property(e => e.RankId);
        builder.Property(e => e.Nick);

        builder.HasOne(x => x.Guild)
            .WithMany(x => x.Members)
            .HasForeignKey(x => x.GuildId);
    }
}