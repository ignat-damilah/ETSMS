using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Foundation.Domain.Entities;

namespace Foundation.Application.Interfaces;

public interface ISkillRepository
{
    Task AddAsync(Skill skill, CancellationToken cancellationToken = default);
    Task<Skill?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Skill>> ListPrimaryAsync(bool? isActive = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<Skill>> ListSecondaryAsync(Guid parentSkillId, bool? isActive = null, CancellationToken cancellationToken = default);
    Task UpdateAsync(Skill skill, CancellationToken cancellationToken = default);
    Task DeleteAsync(Skill skill, CancellationToken cancellationToken = default);
}