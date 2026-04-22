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
                   .ValueGeneratedNever(); // Server-generated via application code (Guid.NewGuid())

            builder.Property(s => s.Name)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(s => s.Category)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(s => s.ParentSkillId)
                   .IsRequired(false);

            builder.Property(s => s.IsActive)
                   .IsRequired()
                   .HasDefaultValue(true);

            // Self-referencing FK: secondary skill → primary skill
            builder.HasOne(s => s.ParentSkill)
                   .WithMany(s => s.ChildSkills)
                   .HasForeignKey(s => s.ParentSkillId)
                   .IsRequired(false)
                   .OnDelete(DeleteBehavior.Restrict);

            // Index for fast look-ups by parent (list children queries)
            builder.HasIndex(s => s.ParentSkillId)
                   .HasDatabaseName("IX_Skills_ParentSkillId");

            // Index for category-filtered list queries
            builder.HasIndex(s => s.Category)
                   .HasDatabaseName("IX_Skills_Category");

            // Composite index supporting active-state + parent queries
            builder.HasIndex(s => new { s.ParentSkillId, s.IsActive })
                   .HasDatabaseName("IX_Skills_ParentSkillId_IsActive");
        });
    }
}
