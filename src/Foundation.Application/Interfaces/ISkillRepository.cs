using Foundation.Domain.Entities;

namespace Foundation.Application.Interfaces;

public interface ISkillRepository
{
    Task<IEnumerable<Skill>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Skill?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Skill skill, CancellationToken cancellationToken = default);
    Task UpdateAsync(Skill skill, CancellationToken cancellationToken = default);
    Task DeleteAsync(Skill skill, CancellationToken cancellationToken = default);
    Task<IEnumerable<Skill>> GetSecondarySkillsAsync(Guid primarySkillId, CancellationToken cancellationToken = default);
    Task<bool> HasActiveSecondariesAsync(Guid primarySkillId, CancellationToken cancellationToken = default);
}
