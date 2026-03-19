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

    public async Task<IEnumerable<SkillTreeNodeDto>> GetTaxonomyAsync(CancellationToken cancellationToken = default)
    {
        var skills = await _repository.GetAllAsync(cancellationToken);

        var skillLookup = skills.ToLookup(s => s.ParentSkillId);
        var rootSkills = skillLookup[null]
            .OrderBy(s => s.Name)
            .Select(s => MapNode(s, skillLookup));

        return rootSkills;
    }

    public async Task<SkillTreeNodeDto?> CreateAsync(SkillCreateDto skill, CancellationToken cancellationToken = default)
    {
        var entity = new Skill
        {
            Id = Guid.NewGuid(),
            Name = skill.Name,
            Description = skill.Description,
            ParentSkillId = skill.ParentSkillId
        };

        await _repository.AddAsync(entity, cancellationToken);
        return MapNode(entity, Array.Empty<Skill>());
    }

    public async Task<SkillTreeNodeDto?> UpdateAsync(Guid id, SkillUpdateDto skill, CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetByIdAsync(id, cancellationToken);

        if (existing is null)
        {
            return null;
        }

        existing.Name = skill.Name;
        existing.Description = skill.Description;
        if (existing.ParentSkillId != skill.ParentSkillId)
        {
            existing.ParentSkillId = skill.ParentSkillId;
            existing.ParentSkill = null;
        }

        await _repository.UpdateAsync(existing, cancellationToken);
        return MapNode(existing, Array.Empty<Skill>());
    }

    public async Task<bool> DeprecateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            return false;
        }

        existing.IsDeprecated = true;
        await _repository.UpdateAsync(existing, cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            return false;
        }

        var hasAssignments = await _repository.HasAssignmentsAsync(id, cancellationToken);
        if (hasAssignments)
        {
            return false;
        }

        await _repository.DeleteAsync(existing, cancellationToken);
        return true;
    }

    private static SkillTreeNodeDto MapNode(Skill skill, ILookup<Guid?, Skill> skillLookup)
    {
        var children = skillLookup[skill.Id]
            .OrderBy(s => s.Name)
            .Select(child => MapNode(child, skillLookup))
            .ToList();

        return new SkillTreeNodeDto
        {
            Id = skill.Id,
            Name = skill.Name,
            Description = skill.Description,
            IsDeprecated = skill.IsDeprecated,
            ParentSkillId = skill.ParentSkillId,
            Children = children
        };
    }
}
