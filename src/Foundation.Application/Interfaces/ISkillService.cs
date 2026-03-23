using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Foundation.Application.DTOs;

namespace Foundation.Application.Interfaces;

public interface ISkillService
{
    Task<SkillDto> CreateAsync(CreateSkillDto dto, string userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<SkillDto>> GetPrimaryAsync(bool? isActive = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<SkillDto>> GetSecondaryAsync(Guid parentSkillId, bool? isActive = null, CancellationToken cancellationToken = default);
    Task<SkillDto> UpdateAsync(UpdateSkillDto dto, string userId, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, string userId, CancellationToken cancellationToken = default);
}