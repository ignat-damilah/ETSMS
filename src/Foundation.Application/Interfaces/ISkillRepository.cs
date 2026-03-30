using Foundation.Domain.Entities;

namespace Foundation.Application.Interfaces;

public interface ISkillRepository
{
    Task AddAsync(Skill skill, CancellationToken cancellationToken = default);

    Task<Skill?> GetByIdAsync(
        Guid id,
        bool includeDeleted = false,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(Skill skill, CancellationToken cancellationToken = default);

    Task<IEnumerable<Skill>> ListAsync(
        bool? isActive = null,
        SkillCategory? category = null,
        Guid? parentSkillId = null,
        bool includeDeleted = false,
        CancellationToken cancellationToken = default);

    Task SoftDeleteAsync(Skill skill, CancellationToken cancellationToken = default);

    Task<bool> HasChildrenAsync(Guid skillId, CancellationToken cancellationToken = default);

    Task<IEnumerable<Skill>> GetChildrenAsync(Guid parentSkillId, CancellationToken cancellationToken = default);

    Task<Skill?> GetParentAsync(Guid skillId, CancellationToken cancellationToken = default);
}
