using Foundation.Application.DTOs;

namespace Foundation.Application.Services;

public interface ISkillService
{
    Task<IEnumerable<SkillDto>> ListAsync(CancellationToken cancellationToken = default);
    Task<SkillDto?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<SkillDto>> GetSecondarySkillsAsync(Guid primarySkillId, CancellationToken cancellationToken = default);
    Task CreateAsync(CreateSkillDto dto, CancellationToken cancellationToken = default);
    Task UpdateAsync(Guid id, UpdateSkillDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
