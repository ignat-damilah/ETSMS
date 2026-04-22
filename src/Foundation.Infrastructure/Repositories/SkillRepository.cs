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

    /// <inheritdoc />
    public async Task<Skill?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Skills
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Skill?> GetByIdWithParentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Skills
            .AsNoTracking()
            .Include(s => s.ParentSkill)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Skill>> GetAllAsync(
        string? category,
        Guid? parentSkillId,
        bool filterByParentSkillId,
        bool? isActive,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Skills
            .AsNoTracking()
            .Include(s => s.ParentSkill)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(s => s.Category == category);
        }

        if (filterByParentSkillId)
        {
            query = query.Where(s => s.ParentSkillId == parentSkillId);
        }

        if (isActive.HasValue)
        {
            query = query.Where(s => s.IsActive == isActive.Value);
        }

        return await query
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Skill>> GetChildrenAsync(
        Guid parentSkillId,
        bool? isActive,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Skills
            .AsNoTracking()
            .Where(s => s.ParentSkillId == parentSkillId)
            .AsQueryable();

        if (isActive.HasValue)
        {
            query = query.Where(s => s.IsActive == isActive.Value);
        }

        return await query
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(Skill skill, CancellationToken cancellationToken = default)
    {
        await _context.Skills.AddAsync(skill, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task UpdateAsync(Skill skill, CancellationToken cancellationToken = default)
    {
        _context.Skills.Update(skill);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Skill skill, CancellationToken cancellationToken = default)
    {
        _context.Skills.Remove(skill);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> HasActiveChildrenAsync(Guid skillId, CancellationToken cancellationToken = default)
    {
        return await _context.Skills
            .AnyAsync(s => s.ParentSkillId == skillId && s.IsActive, cancellationToken);
    }
}
