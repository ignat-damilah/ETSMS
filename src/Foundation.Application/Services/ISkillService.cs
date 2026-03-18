using Foundation.Application.DTOs;

namespace Foundation.Application.Services;

public interface ISkillService
{
    Task<IEnumerable<SkillDto>> GetPrimarySkillsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<SkillDto>> GetSecondarySkillsAsync(Guid primarySkillId, CancellationToken cancellationToken = default);
    Task<SkillDto?> GetSkillAsync(Guid skillId, CancellationToken cancellationToken = default);
    Task AddSkillAsync(CreateSkillDto dto, CancellationToken cancellationToken = default);
    Task DeleteSkillAsync(Guid skillId, CancellationToken cancellationToken = default);
}
