using Foundation.Domain.Entities;

namespace Foundation.Application.Interfaces;

public interface ISkillRepository
{
    Task<Skill?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Skill>> ListAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Skill>> QueryAsync(
        SkillCategory? category,
        Guid? parentSkillId,
        bool? isActive,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task AddAsync(Skill skill, CancellationToken cancellationToken = default);
    Task UpdateAsync(Skill skill, CancellationToken cancellationToken = default);
    Task DeleteAsync(Skill skill, CancellationToken cancellationToken = default);
}
