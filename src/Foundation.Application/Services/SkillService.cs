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
        return skills.Select(ConvertToDto);
    }

    public async Task<SkillDto?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var skill = await _repository.GetByIdAsync(id, cancellationToken);
        return skill is null ? null : ConvertToDto(skill);
    }

    public async Task<IEnumerable<SkillDto>> GetSecondarySkillsAsync(Guid primarySkillId, CancellationToken cancellationToken = default)
    {
        var skills = await _repository.GetSecondarySkillsAsync(primarySkillId, cancellationToken);
        return skills.Select(ConvertToDto);
    }

    public async Task CreateAsync(SkillCreateRequest skill, CancellationToken cancellationToken = default)
    {
        var entity = new Skill
        {
            Id = Guid.NewGuid(),
            Name = skill.Name,
            Category = skill.Category,
            ParentSkillId = skill.ParentSkillId
        };

        await _repository.AddAsync(entity, cancellationToken);
    }

    public async Task UpdateAsync(SkillUpdateRequest skill, CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetByIdAsync(skill.Id, cancellationToken);
        if (existing is null)
        {
            throw new InvalidOperationException("Skill not found.");
        }

        existing.Name = skill.Name;
        existing.Category = skill.Category;
        existing.ParentSkillId = skill.ParentSkillId;

        await _repository.UpdateAsync(existing, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            throw new InvalidOperationException("Skill not found.");
        }

        await _repository.DeleteAsync(existing, cancellationToken);
    }

    private static SkillDto ConvertToDto(Skill skill)
    {
        return new SkillDto
        {
            Id = skill.Id,
            Name = skill.Name,
            Category = skill.Category,
            ParentSkillId = skill.ParentSkillId
        };
    }
}
