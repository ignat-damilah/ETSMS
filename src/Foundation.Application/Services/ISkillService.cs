using Foundation.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Foundation.Application.Services
{
    public interface ISkillService
    {
        Task<IEnumerable<SkillDto>> ListHierarchyAsync(CancellationToken cancellationToken = default);
        Task<SkillDto?> GetHierarchyAsync(Guid id, CancellationToken cancellationToken = default);
        Task<SkillDto> CreateAsync(CreateSkillDto dto, CancellationToken cancellationToken = default);
        Task<SkillDto> UpdateAsync(UpdateSkillDto dto, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}