using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Foundation.Application.DTOs;
using Foundation.Application.Interfaces;
using Foundation.Domain.Entities;

namespace Foundation.Application.Services
{
    public sealed class SkillService : ISkillService
    {
        private readonly ISkillRepository _repository;

        public SkillService(ISkillRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<SkillDto>> ListHierarchyAsync(CancellationToken cancellationToken = default)
        {
            var skills = await _repository.GetPrimarySkillsWithActiveSecondariesAsync(cancellationToken);
            return skills.Select(MapToDto);
        }

        public async Task<SkillDto?> GetHierarchyAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var skill = await _repository.GetWithActiveSecondariesAsync(id, cancellationToken);
            if (skill is null || skill.ParentSkillId != null)
            {
                return null;
            }

            return MapToDto(skill);
        }

        public async Task<SkillDto> CreateAsync(CreateSkillDto dto, CancellationToken cancellationToken = default)
        {
            if (dto.ParentSkillId.HasValue)
            {
                var parent = await _repository.GetByIdAsync(dto.ParentSkillId.Value, cancellationToken);
                if (parent is null || !parent.IsActive || parent.ParentSkillId != null)
                {
                    throw new InvalidOperationException("Parent skill must be an active primary skill.");
                }
            }

            var skill = new Skill
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Category = dto.Category,
                ParentSkillId = dto.ParentSkillId,
                IsActive = true
            };

            await _repository.AddAsync(skill, cancellationToken);
            return MapToDto(skill);
        }

        public async Task<SkillDto> UpdateAsync(UpdateSkillDto dto, CancellationToken cancellationToken = default)
        {
            var skill = await _repository.GetByIdAsync(dto.Id, cancellationToken);
            if (skill is null)
            {
                throw new KeyNotFoundException("Skill not found.");
            }

            if (dto.ParentSkillId.HasValue)
            {
                if (dto.ParentSkillId.Value == dto.Id)
                {
                    throw new InvalidOperationException("Skill cannot be parent of itself.");
                }

                var parent = await _repository.GetByIdAsync(dto.ParentSkillId.Value, cancellationToken);
                if (parent is null || !parent.IsActive || parent.ParentSkillId != null)
                {
                    throw new InvalidOperationException("Parent skill must be an active primary skill.");
                }

                if (await CreatesCircularRelation(dto.Id, dto.ParentSkillId.Value))
                {
                    throw new InvalidOperationException("Circular skill hierarchy detected.");
                }
            }

            if (!dto.IsActive && skill.ParentSkillId == null)
            {
                if (await _repository.HasActiveSecondariesAsync(dto.Id, cancellationToken))
                {
                    throw new InvalidOperationException("Cannot inactivate a primary skill with active secondary skills.");
                }
            }

            skill.Name = dto.Name;
            skill.Category = dto.Category;
            skill.ParentSkillId = dto.ParentSkillId;
            skill.IsActive = dto.IsActive;

            await _repository.UpdateAsync(skill, cancellationToken);
            return MapToDto(skill);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var skill = await _repository.GetByIdAsync(id, cancellationToken);
            if (skill is null)
            {
                throw new KeyNotFoundException("Skill not found.");
            }

            if (skill.ParentSkillId == null)
            {
                if (await _repository.HasActiveSecondariesAsync(id, cancellationToken))
                {
                    throw new InvalidOperationException("Cannot delete a primary skill with active secondary skills.");
                }
            }

            await _repository.DeleteAsync(skill, cancellationToken);
        }

        private async Task<bool> CreatesCircularRelation(Guid skillId, Guid newParentId)
        {
            var current = await _repository.GetByIdAsync(newParentId);
            while (current != null)
            {
                if (current.ParentSkillId == skillId)
                {
                    return true;
                }
                if (current.ParentSkillId == null)
                {
                    break;
                }
                current = await _repository.GetByIdAsync(current.ParentSkillId.Value);
            }

            return false;
        }

        private static SkillDto MapToDto(Skill skill)
        {
            return new SkillDto
            {
                Id = skill.Id,
                Name = skill.Name,
                Category = skill.Category,
                ParentSkillId = skill.ParentSkillId,
                IsActive = skill.IsActive,
                SecondarySkills = skill.SecondarySkills
                    .Where(s => s.IsActive)
                    .Select(MapToDto)
                    .ToList()
            };
        }
    }
}