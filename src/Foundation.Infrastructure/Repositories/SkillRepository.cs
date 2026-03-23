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

    public async Task<Skill?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Skills
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Skill>> ListPrimaryAsync(bool? isActive = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Skills
            .AsNoTracking()
            .Where(s => s.ParentSkillId == null);

        if (isActive.HasValue)
            query = query.Where(s => s.IsActive == isActive.Value);

        return await query
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Skill>> ListSecondaryAsync(Guid parentSkillId, bool? isActive = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Skills
            .AsNoTracking()
            .Where(s => s.ParentSkillId == parentSkillId);

        if (isActive.HasValue)
            query = query.Where(s => s.IsActive == isActive.Value);

        return await query
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);
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
