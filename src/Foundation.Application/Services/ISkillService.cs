using Foundation.Application.DTOs;

namespace Foundation.Application.Services;

public interface ISkillService
{
    Task<SkillDto> CreatePrimaryAsync(string name, string category, CancellationToken cancellationToken = default);
    Task<SkillDto> CreateSecondaryAsync(string name, Guid primarySkillId, CancellationToken cancellationToken = default);
    Task<IEnumerable<SkillDto>> ListSecondaryAsync(Guid primarySkillId, bool includeInactive = false, CancellationToken cancellationToken = default);
    Task DeprecateAsync(Guid skillId, CancellationToken cancellationToken = default);
}
