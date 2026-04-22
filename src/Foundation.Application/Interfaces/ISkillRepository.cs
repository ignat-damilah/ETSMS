using Foundation.Domain.Entities;

namespace Foundation.Application.Interfaces;

public interface ISkillRepository
{
    Task<Skill?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Skill?> GetByIdWithParentAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IEnumerable<Skill>> GetAllAsync(
        string? category,
        Guid? parentSkillId,
        bool filterByParentSkillId,
        bool? isActive,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<Skill>> GetChildrenAsync(
        Guid parentSkillId,
        bool? isActive,
        CancellationToken cancellationToken = default);

    Task AddAsync(Skill skill, CancellationToken cancellationToken = default);

    Task UpdateAsync(Skill skill, CancellationToken cancellationToken = default);

    Task DeleteAsync(Skill skill, CancellationToken cancellationToken = default);

    Task<bool> HasActiveChildrenAsync(Guid skillId, CancellationToken cancellationToken = default);
}
