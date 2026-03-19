using Foundation.Application.DTOs;

namespace Foundation.Application.Interfaces;

public interface ISkillService
{
    Task<IEnumerable<SkillTreeNodeDto>> GetTaxonomyAsync(CancellationToken cancellationToken = default);
    Task<SkillTreeNodeDto?> CreateAsync(SkillCreateDto skill, CancellationToken cancellationToken = default);
    Task<SkillTreeNodeDto?> UpdateAsync(Guid id, SkillUpdateDto skill, CancellationToken cancellationToken = default);
    Task<bool> DeprecateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
