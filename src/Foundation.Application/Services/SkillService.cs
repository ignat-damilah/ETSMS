using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Foundation.Application.DTOs;
using Foundation.Application.Interfaces;
using Foundation.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Services
{
    public sealed class SkillService : ISkillService
    {
        private readonly ISkillRepository _repository;
        private readonly ILogger<SkillService> _logger;

        public SkillService(ISkillRepository repository, ILogger<SkillService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<SkillDto> CreateAsync(CreateSkillDto dto, string userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Skill name must not be empty.", nameof(dto.Name));

            if (dto.ParentSkillId.HasValue)
            {
                var parent = await _repository.GetByIdAsync(dto.ParentSkillId.Value, cancellationToken);
                if (parent == null)
                    throw new KeyNotFoundException($"Parent skill with id '{dto.ParentSkillId}' not found.");
            }

            var existing = dto.ParentSkillId.HasValue
                ? (await _repository.ListSecondaryAsync(dto.ParentSkillId.Value, null, cancellationToken))
                    .FirstOrDefault(s => s.Name == dto.Name && s.Category == dto.Category)
                : (await _repository.ListPrimaryAsync(null, cancellationToken))
                    .FirstOrDefault(s => s.Name == dto.Name && s.Category == dto.Category);

            if (existing != null)
                throw new InvalidOperationException("A skill with the same name, category and parent already exists.");

            var skill = new Skill
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Category = dto.Category,
                ParentSkillId = dto.ParentSkillId,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow
            };

            await _repository.AddAsync(skill, cancellationToken);

            _logger.LogInformation("User {UserId} created skill {SkillId} at {Time}.", userId, skill.Id, DateTime.UtcNow);

            return ToDto(skill);
        }

        public async Task<IEnumerable<SkillDto>> GetPrimaryAsync(bool? isActive = null, CancellationToken cancellationToken = default)
        {
            var skills = await _repository.ListPrimaryAsync(isActive, cancellationToken);
            return skills.Select(ToDto);
        }

        public async Task<IEnumerable<SkillDto>> GetSecondaryAsync(Guid parentSkillId, bool? isActive = null, CancellationToken cancellationToken = default)
        {
            var skills = await _repository.ListSecondaryAsync(parentSkillId, isActive, cancellationToken);
            return skills.Select(ToDto);
        }

        public async Task<SkillDto> UpdateAsync(UpdateSkillDto dto, string userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Skill name must not be empty.", nameof(dto.Name));

            var skill = await _repository.GetByIdAsync(dto.Id, cancellationToken);
            if (skill == null)
                throw new KeyNotFoundException($"Skill with id '{dto.Id}' not found.");

            if (dto.ParentSkillId.HasValue)
            {
                var parent = await _repository.GetByIdAsync(dto.ParentSkillId.Value, cancellationToken);
                if (parent == null)
                    throw new KeyNotFoundException($"Parent skill with id '{dto.ParentSkillId}' not found.");
            }

            var siblings = dto.ParentSkillId.HasValue
                ? await _repository.ListSecondaryAsync(dto.ParentSkillId.Value, null, cancellationToken)
                : await _repository.ListPrimaryAsync(null, cancellationToken);

            if (siblings.Any(s => s.Id != dto.Id && s.Name == dto.Name && s.Category == dto.Category))
                throw new InvalidOperationException("A skill with the same name, category and parent already exists.");

            skill.Name = dto.Name;
            skill.Category = dto.Category;
            skill.ParentSkillId = dto.ParentSkillId;
            skill.IsActive = dto.IsActive;
            skill.ModifiedDate = DateTime.UtcNow;

            await _repository.UpdateAsync(skill, cancellationToken);

            _logger.LogInformation("User {UserId} updated skill {SkillId} at {Time}.", userId, skill.Id, DateTime.UtcNow);

            return ToDto(skill);
        }

        public async Task DeleteAsync(Guid id, string userId, CancellationToken cancellationToken = default)
        {
            var skill = await _repository.GetByIdAsync(id, cancellationToken);
            if (skill == null)
                throw new KeyNotFoundException($"Skill with id '{id}' not found.");

            if (!skill.ParentSkillId.HasValue)
            {
                var activeChildren = await _repository.ListSecondaryAsync(skill.Id, true, cancellationToken);
                if (activeChildren.Any())
                    throw new InvalidOperationException("Cannot delete a primary skill with active secondary skills.");
            }

            await _repository.DeleteAsync(skill, cancellationToken);

            _logger.LogInformation("User {UserId} deleted skill {SkillId} at {Time}.", userId, skill.Id, DateTime.UtcNow);
        }

        private static SkillDto ToDto(Skill s) => new SkillDto
        {
            Id = s.Id,
            Name = s.Name,
            Category = s.Category,
            ParentSkillId = s.ParentSkillId,
            IsActive = s.IsActive,
            CreatedDate = s.CreatedDate,
            ModifiedDate = s.ModifiedDate
        };
    }
}