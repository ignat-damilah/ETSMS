using Foundation.Application.DTOs;
using Foundation.Domain.Entities;

namespace Foundation.Application.Interfaces;

public interface ISkillService
{
    Task<SkillDto> CreateAsync(SkillDto dto, CancellationToken cancellationToken = default);
    Task<SkillDto?> GetAsync(Guid id, bool includeDeleted = false, CancellationToken cancellationToken = default);
    Task<SkillDto> UpdateAsync(Guid id, SkillDto dto, CancellationToken cancellationToken = default);
    Task SoftDeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<SkillDto>> ListAsync(
        bool? isActive = null,
        SkillCategory? category = null,
        Guid? parentSkillId = null,
        bool includeDeleted = false,
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default);
    Task<SkillDto?> GetParentAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<SkillDto>> GetChildrenAsync(Guid id, CancellationToken cancellationToken = default);
}
