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

    public async Task<IEnumerable<Skill>> GetSecondarySkills(Guid primarySkillId, CancellationToken cancellationToken = default)
    {
        return await _context.Skills
            .AsNoTracking()
            .Where(skill => skill.ParentSkillId == primarySkillId)
            .ToListAsync(cancellationToken);
    }
}
