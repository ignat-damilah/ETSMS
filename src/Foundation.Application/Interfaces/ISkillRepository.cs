using Foundation.Domain.Entities;

namespace Foundation.Application.Interfaces;

public interface ISkillRepository
{
    Task AddAsync(Skill skill, CancellationToken cancellationToken = default);
    Task<IEnumerable<Skill>> GetChildren(Guid primarySkillId, CancellationToken cancellationToken = default);
}
