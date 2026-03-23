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
            builder.Property(s => s.Name).IsRequired().HasMaxLength(50);
            builder.Property(s => s.Category).HasMaxLength(100);
            builder.Property(s => s.IsActive).IsRequired();

            builder.HasOne(s => s.ParentSkill)
                   .WithMany(s => s.SecondarySkills)
                   .HasForeignKey(s => s.ParentSkillId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(s => new { s.Name, s.Category }).IsUnique();

            // Global query filter to exclude soft-deleted skills
            builder.HasQueryFilter(s => s.IsActive);
        });
    }
}
