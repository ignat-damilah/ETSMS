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
            builder.Property(s => s.Category).IsRequired().HasMaxLength(200);
            builder.Property(s => s.IsActive).HasDefaultValue(true);

            builder.HasOne<Skill>()
                .WithMany()
                .HasForeignKey(s => s.ParentSkillId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var primarySkills = ChangeTracker.Entries<Skill>()
            .Where(entry => entry.Entity.ParentSkillId is null
                && entry.Property(skill => skill.IsActive).IsModified
                && !entry.Entity.IsActive)
            .Select(entry => entry.Entity.Id)
            .ToList();

        if (primarySkills.Any())
        {
            var secondaries = await Skills
                .Where(skill => skill.ParentSkillId != null && primarySkills.Contains(skill.ParentSkillId.Value))
                .ToListAsync(cancellationToken);

            foreach (var secondary in secondaries)
            {
                secondary.IsActive = false;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
