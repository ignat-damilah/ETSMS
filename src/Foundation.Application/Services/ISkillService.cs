using Foundation.Application.DTOs;

namespace Foundation.Application.Services;

public interface ISkillService
{
    Task<IEnumerable<SkillDto>> ListAsync(Guid? parentSkillId = null, CancellationToken cancellationToken = default);
    Task<SkillDto?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<SkillDto> CreateAsync(SkillDto skill, string userId, CancellationToken cancellationToken = default);
    Task<SkillDto?> UpdateAsync(Guid id, SkillDto skill, string userId, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, string userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<SkillDto>> ListSecondariesAsync(Guid id, CancellationToken cancellationToken = default);
}
