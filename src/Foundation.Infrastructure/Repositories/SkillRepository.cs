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
        => await _context.Skills.AsNoTracking().ToListAsync(cancellationToken);

    public async Task<Skill?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Skills.FindAsync(new object[] { id }, cancellationToken);

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

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Skills.AnyAsync(s => s.Id == id, cancellationToken);

    public async Task<bool> HasActiveSecondariesAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Skills.AnyAsync(s => s.ParentSkillId == id && s.IsActive, cancellationToken);

    public async Task<Skill?> GetWithActiveSecondariesAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Skills
            .Include(s => s.SecondarySkills.Where(sec => sec.IsActive))
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task<IEnumerable<Skill>> GetPrimarySkillsWithActiveSecondariesAsync(CancellationToken cancellationToken = default)
        => await _context.Skills
            .Where(s => s.ParentSkillId == null)
            .Include(s => s.SecondarySkills.Where(sec => sec.IsActive))
            .ToListAsync(cancellationToken);
}