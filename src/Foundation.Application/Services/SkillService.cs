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

    public async Task<IEnumerable<SkillDto>> ListAsync(bool includeInactive = true, CancellationToken cancellationToken = default)
    {
        var skills = await _repository.GetAllAsync(cancellationToken);
        return skills
            .Where(s => includeInactive || s.IsActive)
            .Select(ToDto);
    }

    public async Task<SkillDto?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var skill = await _repository.GetByIdAsync(id, cancellationToken);
        return skill is null ? null : ToDto(skill);
    }

    public async Task<SkillHierarchyDto?> GetHierarchyAsync(Guid id, bool includeInactiveAncestors = true, bool includeInactiveDescendants = true, CancellationToken cancellationToken = default)
    {
        var skill = await _repository.GetByIdAsync(id, cancellationToken);
        if (skill is null)
        {
            return null;
        }

        var ancestors = (await _repository.GetAncestorsAsync(id, cancellationToken))
            .Where(s => includeInactiveAncestors || s.IsActive)
            .Select(ToDto);

        var descendants = (await _repository.GetDescendantsAsync(id, cancellationToken))
            .Where(s => includeInactiveDescendants || s.IsActive)
            .Select(ToDto);

        return new SkillHierarchyDto
        {
            Current = ToDto(skill),
            Ancestors = ancestors,
            Descendants = descendants
        };
    }

    public async Task<SkillDto> CreateAsync(SkillCreateDto createDto, CancellationToken cancellationToken = default)
    {
        var skill = new Skill
        {
            Id = Guid.NewGuid(),
            Name = createDto.Name,
            Category = createDto.Category,
            ParentSkillId = createDto.ParentSkillId,
            IsActive = true
        };

        if (skill.ParentSkillId.HasValue)
        {
            var parent = await _repository.GetByIdAsync(skill.ParentSkillId.Value, cancellationToken);
            if (parent is null)
            {
                throw new InvalidOperationException("Parent skill does not exist.");
            }
        }

        await _repository.AddAsync(skill, cancellationToken);
        return ToDto(skill);
    }

    public async Task<SkillDto> UpdateAsync(Guid id, SkillUpdateDto updateDto, CancellationToken cancellationToken = default)
    {
        var skill = await _repository.GetByIdAsync(id, cancellationToken)
                ?? throw new InvalidOperationException("Skill not found.");

        if (updateDto.ParentSkillId.HasValue && await _repository.IsAncestorAsync(id, updateDto.ParentSkillId.Value, cancellationToken))
        {
            throw new InvalidOperationException("Circular skill parent relationship detected.");
        }

        if (updateDto.ParentSkillId.HasValue)
        {
            var parent = await _repository.GetByIdAsync(updateDto.ParentSkillId.Value, cancellationToken);
            if (parent is null)
            {
                throw new InvalidOperationException("Parent skill does not exist.");
            }
        }

        skill.Name = updateDto.Name;
        skill.Category = updateDto.Category;
        skill.ParentSkillId = updateDto.ParentSkillId;
        skill.IsActive = updateDto.IsActive;

        await _repository.UpdateAsync(skill, cancellationToken);
        return ToDto(skill);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var skill = await _repository.GetByIdAsync(id, cancellationToken)
                ?? throw new InvalidOperationException("Skill not found.");

        if (await _repository.HasActiveChildrenAsync(id, cancellationToken))
        {
            throw new InvalidOperationException("Cannot delete skill while active child skills exist. Deactivate dependents first.");
        }

        await _repository.DeleteAsync(skill, cancellationToken);
    }

    private static SkillDto ToDto(Skill skill)
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
