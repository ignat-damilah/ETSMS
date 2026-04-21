using Foundation.Application.DTOs;

namespace Foundation.Application.Services;

public interface ISkillService
{
    Task<IEnumerable<SkillDto>> ListAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<SkillDto>> GetSecondarySkillsAsync(Guid primarySkillId, CancellationToken cancellationToken = default);
    Task<SkillDto> CreatePrimarySkillAsync(string name, string category, CancellationToken cancellationToken = default);
    Task<SkillDto> CreateSecondarySkillAsync(string name, string category, Guid primarySkillId, CancellationToken cancellationToken = default);
    Task DeactivateSkillAsync(Guid skillId, CancellationToken cancellationToken = default);
}
