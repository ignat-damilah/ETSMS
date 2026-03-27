using Foundation.Application.DTOs;
using Foundation.Domain.Entities;

namespace Foundation.Application.Services;

public interface ISkillService
{
    Task<SkillDto> CreateAsync(SkillCreateDto request, CancellationToken cancellationToken = default);
    Task<SkillDto?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<SkillDto>> ListAsync(
        SkillCategory? category,
        Guid? parentSkillId,
        bool? isActive,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<SkillDto> UpdateAsync(Guid id, SkillUpdateDto request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<SkillDto>> GetChildrenAsync(Guid parentId, CancellationToken cancellationToken = default);
    Task<SkillDto?> GetParentAsync(Guid id, CancellationToken cancellationToken = default);
}
