using Foundation.Domain.Entities;

namespace Foundation.Application.Interfaces;

public interface ISkillRepository
{
    Task<IEnumerable<Skill>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Skill?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IEnumerable<Skill>> GetAncestorsAsync(Guid skillId, CancellationToken cancellationToken = default);

    Task<IEnumerable<Skill>> GetDescendantsAsync(Guid skillId, CancellationToken cancellationToken = default);

    Task<bool> HasActiveChildrenAsync(Guid skillId, CancellationToken cancellationToken = default);

    Task<bool> IsAncestorAsync(Guid ancestorId, Guid descendantId, CancellationToken cancellationToken = default);

    Task AddAsync(Skill skill, CancellationToken cancellationToken = default);
    Task UpdateAsync(Skill skill, CancellationToken cancellationToken = default);
    Task DeleteAsync(Skill skill, CancellationToken cancellationToken = default);
}
