using Foundation.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Foundation.Application.Interfaces
{
    public interface ISkillRepository
    {
        Task<IEnumerable<Skill>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Skill?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task AddAsync(Skill skill, CancellationToken cancellationToken = default);
        Task UpdateAsync(Skill skill, CancellationToken cancellationToken = default);
        Task DeleteAsync(Skill skill, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> HasActiveSecondariesAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Skill?> GetWithActiveSecondariesAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Skill>> GetPrimarySkillsWithActiveSecondariesAsync(CancellationToken cancellationToken = default);
    }
}