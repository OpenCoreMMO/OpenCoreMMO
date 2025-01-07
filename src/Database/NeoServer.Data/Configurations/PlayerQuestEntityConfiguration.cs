using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeoServer.Data.Entities;

namespace NeoServer.Data.Configurations;

public class PlayerQuestEntityConfiguration : IEntityTypeConfiguration<PlayerQuestEntity>
{
    public void Configure(EntityTypeBuilder<PlayerQuestEntity> builder)
    {
        builder.HasKey(e => new { e.ActionId, e.UniqueId });

        builder.Property(e => e.ActionId).IsRequired();
        builder.Property(e => e.UniqueId).IsRequired();
        builder.Property(e => e.Name);
        builder.Property(e => e.Done);
        builder.Property(e => e.Group);
        builder.Property(e => e.GroupKey);

        builder.Property(e => e.PlayerId).IsRequired();

        builder.HasOne(e => e.Player)
            .WithMany()
            .HasForeignKey(x => x.PlayerId);
    }
}