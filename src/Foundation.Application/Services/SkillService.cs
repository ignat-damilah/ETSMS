using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Foundation.Application.DTOs;
using Foundation.Application.Interfaces;
using Foundation.Domain.Entities;

namespace Foundation.Application.Services;

public sealed class SkillService : ISkillService
{
    private readonly ISkillRepository _repository;

    public SkillService(ISkillRepository repository)
    {
        _repository = repository;
    }

    public async Task<SkillDto> CreatePrimaryAsync(string name, string category, CancellationToken cancellationToken = default)
    {
        var skill = new Skill
        {
            Id = Guid.NewGuid(),
            Name = name,
            Category = category,
            IsActive = true
        };

        await _repository.AddAsync(skill, cancellationToken);

        return Map(skill);
    }

    public async Task<SkillDto> CreateSecondaryAsync(string name, Guid primarySkillId, CancellationToken cancellationToken = default)
    {
        var primarySkill = await _repository.GetByIdAsync(primarySkillId, cancellationToken);
        if (primarySkill is null || primarySkill.ParentSkillId is not null)
        {
            throw new InvalidOperationException("Primary skill not found.");
        }

        var skill = new Skill
        {
            Id = Guid.NewGuid(),
            Name = name,
            Category = primarySkill.Category,
            ParentSkillId = primarySkillId,
            IsActive = true
        };

        await _repository.AddAsync(skill, cancellationToken);

        return Map(skill);
    }

    public async Task<IEnumerable<SkillDto>> ListSecondaryAsync(Guid primarySkillId, bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var secondarySkills = await _repository.GetSecondarySkillsAsync(primarySkillId, includeInactive, cancellationToken);
        return secondarySkills.Select(Map);
    }

    public async Task DeprecateAsync(Guid skillId, CancellationToken cancellationToken = default)
    {
        var skill = await _repository.GetByIdAsync(skillId, cancellationToken);
        if (skill is null)
        {
            throw new InvalidOperationException("Skill not found.");
        }

        skill.IsActive = false;
        await _repository.UpdateAsync(skill, cancellationToken);
    }

    private static SkillDto Map(Skill skill)
    {
        return new SkillDto
        {
            Id = skill.Id,
            Name = skill.Name,
            Category = skill.Category,
            ParentSkillId = skill.ParentSkillId,
            IsActive = skill.IsActive
        };
    }
}
