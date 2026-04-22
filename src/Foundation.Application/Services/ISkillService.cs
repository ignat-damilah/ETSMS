using Foundation.Application.DTOs;

namespace Foundation.Application.Services;

public interface ISkillService
{
    Task<(SkillDto Skill, string Location)> CreateAsync(CreateSkillRequest request, CancellationToken cancellationToken = default);
    Task<SkillDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<SkillDto> UpdateAsync(Guid id, UpdateSkillRequest request, CancellationToken cancellationToken = default);
    Task SoftDeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<SkillDto>> ListAsync(SkillFilterParams filters, CancellationToken cancellationToken = default);
    Task<IEnumerable<SkillDto>> GetChildrenAsync(Guid id, bool? isActive, bool? isDeleted, CancellationToken cancellationToken = default);
    Task<SkillDto?> GetParentAsync(Guid id, CancellationToken cancellationToken = default);
}
