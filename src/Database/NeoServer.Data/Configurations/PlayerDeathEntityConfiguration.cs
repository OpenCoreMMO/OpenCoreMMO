using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeoServer.Data.Entities;

namespace NeoServer.Data.Configurations;

public class PlayerDeathEntityConfiguration : IEntityTypeConfiguration<PlayerDeathEntity>
{
    public void Configure(EntityTypeBuilder<PlayerDeathEntity> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).ValueGeneratedOnAdd().IsRequired();

        builder.Property(e => e.PlayerId).IsRequired();
        builder.Property(e => e.DeathDateTime).IsRequired();
        builder.Property(e => e.Level).IsRequired();
        builder.Property(e => e.Unjustified).IsRequired();
        builder.Property(e => e.DeathLocation).IsRequired();
        builder.Property(e => e.ExperienceLost);
        builder.Property(e => e.SkillAxeLost);
        builder.Property(e => e.SkillClubLost);
        builder.Property(e => e.SkillDistanceLost);
        builder.Property(e => e.SkillFishingLost);
        builder.Property(e => e.SkillFistLost);
        builder.Property(e => e.SkillSwordLost);
        builder.Property(e => e.SkillMagicLevelLost);

        builder.HasMany(x => x.Killers);

        builder.HasOne(x => x.Player).WithMany(x => x.Deaths).HasForeignKey(d => d.PlayerId);
    }
}