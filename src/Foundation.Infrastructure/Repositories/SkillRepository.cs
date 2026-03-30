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

    public async Task AddAsync(Skill skill, CancellationToken cancellationToken = default)
    {
        await _context.Skills.AddAsync(skill, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Skill?> GetByIdAsync(Guid id, bool includeDeleted = false, CancellationToken cancellationToken = default)
    {
        var query = _context.Skills.AsQueryable();
        if (!includeDeleted) query = query.Where(s => !s.IsDeleted);
        return await query
            .Include(s => s.ParentSkill)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task UpdateAsync(Skill skill, CancellationToken cancellationToken = default)
    {
        _context.Skills.Update(skill);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<Skill>> ListAsync(bool? isActive = null, SkillCategory? category = null, Guid? parentSkillId = null, bool includeDeleted = false, CancellationToken cancellationToken = default)
    {
        var query = _context.Skills.AsQueryable();
        if (!includeDeleted) query = query.Where(s => !s.IsDeleted);
        if (isActive.HasValue) query = query.Where(s => s.IsActive == isActive.Value);
        if (category.HasValue) query = query.Where(s => s.Category == category.Value);
        if (parentSkillId.HasValue) query = query.Where(s => s.ParentSkillId == parentSkillId.Value);

        return await query
            .Include(s => s.ParentSkill)
            .ToListAsync(cancellationToken);
    }

    public async Task SoftDeleteAsync(Skill skill, CancellationToken cancellationToken = default)
    {
        skill.IsDeleted = true;
        await UpdateAsync(skill, cancellationToken);
    }

    public async Task<bool> HasChildrenAsync(Guid skillId, CancellationToken cancellationToken = default)
    {
        return await _context.Skills.AnyAsync(s => s.ParentSkillId == skillId && !s.IsDeleted, cancellationToken);
    }

    public async Task<IEnumerable<Skill>> GetChildrenAsync(Guid parentSkillId, CancellationToken cancellationToken = default)
    {
        return await _context.Skills
            .Where(s => s.ParentSkillId == parentSkillId && !s.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task<Skill?> GetParentAsync(Guid skillId, CancellationToken cancellationToken = default)
    {
        var skill = await _context.Skills.FindAsync(new object[] { skillId }, cancellationToken: cancellationToken);
        if (skill?.ParentSkillId == null)
            return null;

        return await _context.Skills.FindAsync(new object[] { skill.ParentSkillId.Value }, cancellationToken: cancellationToken);
    }
}
