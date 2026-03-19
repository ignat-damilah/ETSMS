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

    public async Task<IEnumerable<SkillDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        var skills = await _repository.GetAllAsync(cancellationToken);
        return skills.Select(Map);
    }

    public async Task<SkillDto?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var skill = await _repository.GetByIdAsync(id, cancellationToken);
        return skill is null ? null : Map(skill);
    }

    public async Task<IEnumerable<SkillDto>> GetSecondarySkillsAsync(Guid primarySkillId, CancellationToken cancellationToken = default)
    {
        var skills = await _repository.GetSecondarySkillsAsync(primarySkillId, cancellationToken);
        return skills.Select(Map);
    }

    public async Task CreateAsync(CreateSkillDto dto, CancellationToken cancellationToken = default)
    {
        ValidateFields(dto.Name, dto.Category);
        await ValidateParentSkillAsync(dto.ParentSkillId, null, cancellationToken);

        var skill = new Skill
        {
            Id = Guid.NewGuid(),
            Name = dto.Name.Trim(),
            Category = dto.Category.Trim(),
            ParentSkillId = dto.ParentSkillId
        };

        await _repository.AddAsync(skill, cancellationToken);
    }

    public async Task UpdateAsync(Guid id, UpdateSkillDto dto, CancellationToken cancellationToken = default)
    {
        ValidateFields(dto.Name, dto.Category);

        var existing = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("Skill not found.");

        await ValidateParentSkillAsync(dto.ParentSkillId, id, cancellationToken);

        if (existing.ParentSkillId is null && dto.ParentSkillId is not null)
        {
            var hasSecondaries = await _repository.HasActiveSecondariesAsync(id, cancellationToken);
            if (hasSecondaries)
            {
                throw new InvalidOperationException("Primary skill has active secondary skills and cannot be converted.");
            }
        }

        existing.Name = dto.Name.Trim();
        existing.Category = dto.Category.Trim();
        existing.ParentSkillId = dto.ParentSkillId;

        await _repository.UpdateAsync(existing, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("Skill not found.");

        if (existing.ParentSkillId is null)
        {
            var hasSecondaries = await _repository.HasActiveSecondariesAsync(id, cancellationToken);
            if (hasSecondaries)
            {
                throw new InvalidOperationException("Primary skill has active secondary skills.");
            }
        }

        await _repository.DeleteAsync(existing, cancellationToken);
    }

    private static void ValidateFields(string name, string category)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("Skill name is required.");
        }

        if (string.IsNullOrWhiteSpace(category))
        {
            throw new InvalidOperationException("Skill category is required.");
        }
    }

    private async Task ValidateParentSkillAsync(Guid? parentSkillId, Guid? currentSkillId, CancellationToken cancellationToken)
    {
        if (parentSkillId is null)
        {
            return;
        }

        if (parentSkillId == Guid.Empty)
        {
            throw new InvalidOperationException("ParentSkillId must be a valid skill identifier.");
        }

        if (currentSkillId is not null && parentSkillId == currentSkillId)
        {
            throw new InvalidOperationException("Skill cannot reference itself as a parent.");
        }

        var parent = await _repository.GetByIdAsync(parentSkillId.Value, cancellationToken);
        if (parent is null || parent.ParentSkillId is not null || parent.IsDeleted)
        {
            throw new InvalidOperationException("Secondary skills must reference an existing primary skill.");
        }
    }

    private static SkillDto Map(Skill skill) => new()
    {
        Id = skill.Id,
        Name = skill.Name,
        Category = skill.Category,
        ParentSkillId = skill.ParentSkillId,
        IsDeleted = skill.IsDeleted
    };
}
