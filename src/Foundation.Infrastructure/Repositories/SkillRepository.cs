using Foundation.Application.DTOs;
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

    public async Task<Skill?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Skills
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<Skill?> GetByIdWithParentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Skills
            .AsNoTracking()
            .Include(s => s.ParentSkill)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Skill>> GetAllAsync(
        SkillFilterParams filters,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Skills
            .AsNoTracking()
            .Include(s => s.ParentSkill)
            .AsQueryable();

        // Default: exclude soft-deleted unless caller explicitly requests them
        var isDeletedFilter = filters.IsDeleted ?? false;
        query = query.Where(s => s.IsDeleted == isDeletedFilter);

        if (!string.IsNullOrWhiteSpace(filters.Category))
            query = query.Where(s => s.Category == filters.Category);

        if (filters.ParentSkillId.HasValue)
            query = query.Where(s => s.ParentSkillId == filters.ParentSkillId.Value);

        if (filters.IsActive.HasValue)
            query = query.Where(s => s.IsActive == filters.IsActive.Value);

        return await query
            .OrderBy(s => s.Category)
            .ThenBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Skill>> GetChildrenAsync(
        Guid parentSkillId,
        bool? isActive,
        bool? isDeleted,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Skills
            .AsNoTracking()
            .Include(s => s.ParentSkill)
            .Where(s => s.ParentSkillId == parentSkillId);

        // Default: exclude soft-deleted unless caller explicitly requests them
        var isDeletedFilter = isDeleted ?? false;
        query = query.Where(s => s.IsDeleted == isDeletedFilter);

        if (isActive.HasValue)
            query = query.Where(s => s.IsActive == isActive.Value);

        return await query
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> HasActiveUndeletedChildrenAsync(
        Guid skillId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Skills
            .AnyAsync(
                s => s.ParentSkillId == skillId && s.IsActive && !s.IsDeleted,
                cancellationToken);
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
}
