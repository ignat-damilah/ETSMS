using Foundation.Domain.Entities;

namespace Foundation.Application.Interfaces;

public interface ISkillRepository
{
    Task AddAsync(Skill skill, CancellationToken cancellationToken = default);
    Task UpdateAsync(Skill skill, CancellationToken cancellationToken = default);
    Task DeleteAsync(Skill skill, CancellationToken cancellationToken = default);
    Task<Skill?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Skill>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Skill>> FilterAsync(SkillCategory? category, Guid? parentSkillId, bool? isActive, CancellationToken cancellationToken = default);
    Task<IEnumerable<Skill>> GetChildrenAsync(Guid parentSkillId, CancellationToken cancellationToken = default);
    Task<bool> HasActiveChildrenAsync(Guid parentSkillId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}
