using Foundation.Domain.Entities;

namespace Foundation.Application.Interfaces;

public interface ISkillRepository
{
    Task<Skill?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IEnumerable<Skill>> GetPrimarySkillsAsync(CancellationToken cancellationToken = default);

    Task<IEnumerable<Skill>> GetSecondarySkillsAsync(Guid primarySkillId, CancellationToken cancellationToken = default);

    Task AddAsync(Skill skill, CancellationToken cancellationToken = default);

    Task DeleteAsync(Skill skill, CancellationToken cancellationToken = default);
}
