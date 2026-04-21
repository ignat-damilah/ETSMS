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

    public async Task<IEnumerable<SkillDto>> GetSecondarySkillsAsync(Guid primarySkillId, CancellationToken cancellationToken = default)
    {
        var skills = await _repository.GetSecondarySkillsAsync(primarySkillId, cancellationToken);
        return skills.Select(Map);
    }

    public async Task<SkillDto> CreatePrimarySkillAsync(string name, string category, CancellationToken cancellationToken = default)
    {
        var skill = new Skill
        {
            Id = Guid.NewGuid(),
            Name = name,
            Category = category,
            ParentSkillId = null,
            IsActive = true
        };

        await _repository.AddAsync(skill, cancellationToken);
        return Map(skill);
    }

    public async Task<SkillDto> CreateSecondarySkillAsync(string name, string category, Guid primarySkillId, CancellationToken cancellationToken = default)
    {
        var primarySkill = await _repository.GetAsync(primarySkillId, cancellationToken);
        if (primarySkill is null)
        {
            throw new InvalidOperationException("Primary skill not found.");
        }

        var skill = new Skill
        {
            Id = Guid.NewGuid(),
            Name = name,
            Category = category,
            ParentSkillId = primarySkillId,
            IsActive = true
        };

        await _repository.AddAsync(skill, cancellationToken);
        return Map(skill);
    }

    public async Task DeactivateSkillAsync(Guid skillId, CancellationToken cancellationToken = default)
    {
        var skill = await _repository.GetAsync(skillId, cancellationToken);
        if (skill is null)
        {
            throw new InvalidOperationException("Skill not found.");
        }

        skill.IsActive = false;
        await _repository.UpdateAsync(skill, cancellationToken);
    }

    private static SkillDto Map(Skill skill) => new()
    {
        Id = skill.Id,
        Name = skill.Name,
        Category = skill.Category,
        ParentSkillId = skill.ParentSkillId,
        IsActive = skill.IsActive
    };
}
