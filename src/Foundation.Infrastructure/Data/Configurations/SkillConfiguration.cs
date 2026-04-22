using Foundation.Domain.Entities;
using Foundation.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Foundation.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core fluent configuration for the <see cref="Skill"/> entity / <c>Skills</c> table.
/// </summary>
internal sealed class SkillConfiguration : IEntityTypeConfiguration<Skill>
{
    public void Configure(EntityTypeBuilder<Skill> builder)
    {
        builder.ToTable("Skills");

        // Primary key
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id)
               .ValueGeneratedOnAdd();

        // Name — required, capped at a sensible length
        builder.Property(s => s.Name)
               .IsRequired()
               .HasMaxLength(200);

        // Category — stored as its integer value
        builder.Property(s => s.Category)
               .IsRequired()
               .HasConversion<int>();

        // ParentSkillId — nullable; self-referencing FK
        builder.Property(s => s.ParentSkillId)
               .IsRequired(false);

        // IsActive — required; default true at the database level (FR-7 / A-5)
        builder.Property(s => s.IsActive)
               .IsRequired()
               .HasDefaultValue(true);

        // Self-referencing FK: ParentSkillId → Skills.Id
        // DeleteBehavior.Restrict prevents cascade-delete of child skills (NFR-3)
        builder.HasOne(s => s.ParentSkill)
               .WithMany(s => s.ChildSkills)
               .HasForeignKey(s => s.ParentSkillId)
               .IsRequired(false)
               .OnDelete(DeleteBehavior.Restrict);

        // Unique index: Name must be unique within the same Category + ParentSkillId scope (A-4)
        // NULL ParentSkillId values participate correctly in a partial unique index via
        // the filter expression so that multiple primary skills in the same category can
        // share a name only if they have different ParentSkillIds — in practice each
        // (Name, Category, ParentSkillId) triple must be unique.
        builder.HasIndex(s => new { s.Name, s.Category, s.ParentSkillId })
               .IsUnique()
               .HasDatabaseName("IX_Skills_Name_Category_ParentSkillId");
    }
}
