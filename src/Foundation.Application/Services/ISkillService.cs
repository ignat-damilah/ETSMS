using Foundation.Application.DTOs;

namespace Foundation.Application.Services;

public interface ISkillService
{
    Task<IEnumerable<SkillDto>> ListAsync(bool includeInactive = true, CancellationToken cancellationToken = default);
    Task<SkillDto?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<SkillHierarchyDto?> GetHierarchyAsync(Guid id, bool includeInactiveAncestors = true, bool includeInactiveDescendants = true, CancellationToken cancellationToken = default);
    Task<SkillDto> CreateAsync(SkillCreateDto createDto, CancellationToken cancellationToken = default);
    Task<SkillDto> UpdateAsync(Guid id, SkillUpdateDto updateDto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
