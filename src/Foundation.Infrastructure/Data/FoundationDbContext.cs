using Foundation.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Foundation.Infrastructure.Data;

public sealed class FoundationDbContext : DbContext
{
    public FoundationDbContext(DbContextOptions<FoundationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Skill> Skills => Set<Skill>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Employee>(builder =>
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Email).IsRequired().HasMaxLength(250);
            builder.Property(e => e.Role).HasMaxLength(100);
        });

        modelBuilder.Entity<Skill>(builder =>
        {
            builder.HasKey(s => s.Id);

            builder.Property(s => s.Id)
                .ValueGeneratedNever(); // Server-generated via application (Guid.NewGuid)

            builder.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(s => s.Category)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(s => s.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(s => s.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            // Self-referencing FK: ParentSkillId → Skill.Id
            builder.HasOne(s => s.ParentSkill)
                .WithMany(s => s.ChildSkills)
                .HasForeignKey(s => s.ParentSkillId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes for common query patterns
            builder.HasIndex(s => s.ParentSkillId);
            builder.HasIndex(s => new { s.Category, s.IsDeleted });
            builder.HasIndex(s => new { s.IsActive, s.IsDeleted });

            builder.ToTable("Skills", t =>
            {
                // DB-level check: a skill can only be the parent of another skill if it is itself
                // a primary skill (ParentSkillId IS NULL). This is enforced in the app layer;
                // the FK + application validation together satisfy the two-level hierarchy rule.

                // Prevent a skill from referencing itself as its own parent
                t.HasCheckConstraint("CK_Skills_NoSelfReference", "\"Id\" <> \"ParentSkillId\"");
            });
        });
    }
}
