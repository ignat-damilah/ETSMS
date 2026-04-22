using Foundation.Application.Interfaces;
using Foundation.Domain.Entities;
using Foundation.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Foundation.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of <see cref="ISkillRepository"/>.
/// </summary>
public sealed class SkillRepository : ISkillRepository
{
    private readonly FoundationDbContext _context;

    public SkillRepository(FoundationDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Skill>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Skills
            .AsNoTracking()
            .Where(s => s.IsActive)
            .OrderBy(s => s.Category)
            .ThenBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Skill>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Skills
            .AsNoTracking()
            .OrderBy(s => s.Category)
            .ThenBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Skill>> GetPrimarySkillsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Skills
            .AsNoTracking()
            .Where(s => s.ParentSkillId == null && s.IsActive)
            .OrderBy(s => s.Category)
            .ThenBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Skill>> GetSecondarySkillsAsync(
        Guid primarySkillId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Skills
            .AsNoTracking()
            .Where(s => s.ParentSkillId == primarySkillId && s.IsActive)
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Skill?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Inactive skills are intentionally included so that referential integrity
        // with future employee assessments is preserved (FR-6 / NFR-3).
        return await _context.Skills
            .AsNoTracking()
            .Include(s => s.ChildSkills)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
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
}
