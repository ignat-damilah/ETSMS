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

    public async Task<IEnumerable<SkillDto>> GetPrimarySkillsAsync(CancellationToken cancellationToken = default)
    {
        var skills = await _repository.GetPrimarySkillsAsync(cancellationToken);
        return skills.Select(ToDto);
    }

    public async Task<IEnumerable<SkillDto>> GetSecondarySkillsAsync(Guid primarySkillId, CancellationToken cancellationToken = default)
    {
        var skills = await _repository.GetSecondarySkillsAsync(primarySkillId, cancellationToken);
        return skills.Select(ToDto);
    }

    public async Task<SkillDto?> GetSkillAsync(Guid skillId, CancellationToken cancellationToken = default)
    {
        var skill = await _repository.GetByIdAsync(skillId, cancellationToken);
        if (skill is null)
        {
            return null;
        }

        return ToDto(skill);
    }

    public async Task AddSkillAsync(CreateSkillDto dto, CancellationToken cancellationToken = default)
    {
        var skill = new Skill
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Category = dto.Category,
            ParentSkillId = dto.ParentSkillId,
            IsActive = dto.IsActive
        };

        await _repository.AddAsync(skill, cancellationToken);
    }

    public async Task DeleteSkillAsync(Guid skillId, CancellationToken cancellationToken = default)
    {
        var skill = await _repository.GetByIdAsync(skillId, cancellationToken);
        if (skill is null)
        {
            return;
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
