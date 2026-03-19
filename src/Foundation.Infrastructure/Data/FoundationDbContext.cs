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
    public DbSet<SkillAssignment> SkillAssignments => Set<SkillAssignment>();

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
            builder.Property(s => s.Name).IsRequired().HasMaxLength(250);
            builder.Property(s => s.Description).HasMaxLength(1000);
            builder.Property(s => s.IsDeprecated).HasDefaultValue(false);
            builder.HasMany(s => s.ChildSkills)
                .WithOne(s => s.ParentSkill)
                .HasForeignKey(s => s.ParentSkillId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SkillAssignment>(builder =>
        {
            builder.HasKey(sa => sa.Id);
            builder.Property(sa => sa.AssignmentType).IsRequired().HasMaxLength(100);
            builder.HasOne(sa => sa.Skill)
                .WithMany(s => s.Assignments)
                .HasForeignKey(sa => sa.SkillId);
        });
    }
}
