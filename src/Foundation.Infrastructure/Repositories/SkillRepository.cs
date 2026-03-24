using Foundation.Application.Interfaces;
using Foundation.Domain.Entities;
using Foundation.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Foundation.Infrastructure.Repositories;

public sealed class SkillRepository : ISkillRepository
{
    private readonly FoundationDbContext _context;

    public SkillRepository(FoundationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Skill>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Skills
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Skill?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Skills
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Skill>> GetAncestorsAsync(Guid skillId, CancellationToken cancellationToken = default)
    {
        var skills = await _context.Skills
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var lookup = skills.ToDictionary(s => s.Id);
        var ancestors = new List<Skill>();
        var currentId = skillId;

        while (lookup.TryGetValue(currentId, out var current) && current.ParentSkillId is { } parentId)
        {
            if (!lookup.TryGetValue(parentId, out var parent))
            {
                break;
            }

            ancestors.Add(parent);
            currentId = parent.Id;
        }

        return ancestors;
    }

    public async Task<IEnumerable<Skill>> GetDescendantsAsync(Guid skillId, CancellationToken cancellationToken = default)
    {
        var skills = await _context.Skills
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var childLookup = skills
            .Where(s => s.ParentSkillId.HasValue)
            .GroupBy(s => s.ParentSkillId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        var descendants = new List<Skill>();
        var queue = new Queue<Guid>();
        queue.Enqueue(skillId);

        while (queue.Count > 0)
        {
            var currentId = queue.Dequeue();
            if (!childLookup.TryGetValue(currentId, out var children))
            {
                continue;
            }

            foreach (var child in children)
            {
                descendants.Add(child);
                queue.Enqueue(child.Id);
            }
        }

        return descendants;
    }

    public async Task<bool> HasActiveChildrenAsync(Guid skillId, CancellationToken cancellationToken = default)
    {
        return await _context.Skills
            .AnyAsync(s => s.ParentSkillId == skillId && s.IsActive, cancellationToken);
    }

    public async Task<bool> IsAncestorAsync(Guid ancestorId, Guid descendantId, CancellationToken cancellationToken = default)
    {
        var ancestors = await GetAncestorsAsync(descendantId, cancellationToken);
        return ancestors.Any(a => a.Id == ancestorId);
    }

    public async Task AddAsync(Skill skill, CancellationToken cancellationToken = default)
    {
        await _context.Skills.AddAsync(skill, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Skill skill, CancellationToken cancellationToken = default)
    {
        _context.Skills.Update(skill);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Skill skill, CancellationToken cancellationToken = default)
    {
        _context.Skills.Remove(skill);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
