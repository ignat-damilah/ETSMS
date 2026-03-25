using Foundation.Application.Interfaces;
using Foundation.Domain.Entities;
using Foundation.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;

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

    public async Task DeleteAsync(Skill skill, CancellationToken cancellationToken = default)
    {
        skill.IsActive = false;
        _context.Skills.Update(skill);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<Skill>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Skills
            .Include(s => s.ParentSkill)
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Skill?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Skills
            .Include(s => s.ChildSkills)
            .Include(s => s.ParentSkill)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Skill>> FilterAsync(SkillCategory? category, Guid? parentSkillId, bool? isActive, CancellationToken cancellationToken = default)
    {
        var query = _context.Skills.AsQueryable();

        if (category.HasValue)
        {
            query = query.Where(s => s.Category == category.Value);
        }

        if (parentSkillId.HasValue)
        {
            query = query.Where(s => s.ParentSkillId == parentSkillId.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(s => s.IsActive == isActive.Value);
        }

        return await query
            .AsNoTracking()
            .Include(s => s.ParentSkill)
            .Include(s => s.ChildSkills)
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Skill>> GetChildrenAsync(Guid parentSkillId, CancellationToken cancellationToken = default)
    {
        return await _context.Skills
            .Where(s => s.ParentSkillId == parentSkillId)
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> HasActiveChildrenAsync(Guid parentSkillId, CancellationToken cancellationToken = default)
    {
        return await _context.Skills
            .AnyAsync(s => s.ParentSkillId == parentSkillId && s.IsActive, cancellationToken);
    }

    public async Task UpdateAsync(Skill skill, CancellationToken cancellationToken = default)
    {
        _context.Skills.Update(skill);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Skills.AnyAsync(s => s.Id == id, cancellationToken);
    }
}
