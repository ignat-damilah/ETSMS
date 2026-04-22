using Foundation.Application.DTOs;
using Foundation.Domain.Entities;

namespace Foundation.Application.Interfaces;

public interface ISkillRepository
{
    Task<Skill?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Skill?> GetByIdWithParentAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Skill>> GetAllAsync(SkillFilterParams filters, CancellationToken cancellationToken = default);
    Task<IEnumerable<Skill>> GetChildrenAsync(Guid parentSkillId, bool? isActive, bool? isDeleted, CancellationToken cancellationToken = default);
    Task<bool> HasActiveUndeletedChildrenAsync(Guid skillId, CancellationToken cancellationToken = default);
    Task AddAsync(Skill skill, CancellationToken cancellationToken = default);
    Task UpdateAsync(Skill skill, CancellationToken cancellationToken = default);
}
