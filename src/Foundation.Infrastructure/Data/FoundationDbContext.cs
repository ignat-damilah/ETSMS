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
            builder.Property(s => s.Name).IsRequired().HasMaxLength(250);
            builder.Property(s => s.Category).IsRequired().HasMaxLength(100);
            builder.HasIndex(s => new { s.Name, s.Category }).IsUnique();

            builder.HasOne(s => s.ParentSkill)
                .WithMany(p => p.SecondarySkills)
                .HasForeignKey(s => s.ParentSkillId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(s => s.IsDeleted).HasDefaultValue(false);
            builder.Property(s => s.CreatedAt).HasDefaultValueSql("now()");
        });
    }
}
