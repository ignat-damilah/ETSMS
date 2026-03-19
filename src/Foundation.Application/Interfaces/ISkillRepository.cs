using Foundation.Domain.Entities;

namespace Foundation.Application.Interfaces;

public interface ISkillRepository
{
    Task<IEnumerable<Skill>> GetSecondarySkills(Guid primarySkillId, CancellationToken cancellationToken = default);
}
