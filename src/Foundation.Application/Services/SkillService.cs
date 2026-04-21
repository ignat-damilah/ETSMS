using System;
using System.Collections.Generic;
using Foundation.Application.DTOs;
using Foundation.Application.Interfaces;
using Foundation.Domain.Entities;

namespace Foundation.Application.Services;

public interface ISkillService
{
    Task<IEnumerable<Skill>> ListPrimarySkillsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Skill>> GetSecondarySkillsAsync(Guid primarySkillId, CancellationToken cancellationToken = default);
    Task<Skill> CreatePrimarySkillAsync(CreatePrimarySkillRequest request, CancellationToken cancellationToken = default);
    Task<Skill> CreateSecondarySkillAsync(CreateSecondarySkillRequest request, CancellationToken cancellationToken = default);
    Task<Skill?> DeactivateSkillAsync(Guid skillId, CancellationToken cancellationToken = default);
}

public sealed class SkillService : ISkillService
{
    private readonly ISkillRepository _repository;

    public SkillService(ISkillRepository repository)
    {
        _repository = repository;
    }

    public Task<IEnumerable<Skill>> ListPrimarySkillsAsync(CancellationToken cancellationToken = default)
        => _repository.GetAllAsync(cancellationToken);

    public Task<IEnumerable<Skill>> GetSecondarySkillsAsync(Guid primarySkillId, CancellationToken cancellationToken = default)
        => _repository.GetChildSkillsAsync(primarySkillId, cancellationToken);

    public async Task<Skill> CreatePrimarySkillAsync(CreatePrimarySkillRequest request, CancellationToken cancellationToken = default)
    {
        var skill = new Skill
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Category = request.Category,
            ParentSkillId = null,
            IsActive = true
        };

        await _repository.AddAsync(skill, cancellationToken);
        return skill;
    }

    public async Task<Skill> CreateSecondarySkillAsync(CreateSecondarySkillRequest request, CancellationToken cancellationToken = default)
    {
        var parentSkill = await _repository.GetByIdAsync(request.PrimarySkillId, cancellationToken);
        if (parentSkill is null)
        {
            throw new InvalidOperationException("The specified primary skill does not exist.");
        }

        if (parentSkill.ParentSkillId is not null)
        {
            throw new InvalidOperationException("The specified parent skill is not a primary skill.");
        }

        var skill = new Skill
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Category = request.Category,
            ParentSkillId = request.PrimarySkillId,
            IsActive = true
        };

        await _repository.AddAsync(skill, cancellationToken);
        return skill;
    }

    public async Task<Skill?> DeactivateSkillAsync(Guid skillId, CancellationToken cancellationToken = default)
    {
        var skill = await _repository.GetByIdAsync(skillId, cancellationToken);
        if (skill is null)
        {
            return null;
        }

        if (!skill.IsActive)
        {
            return skill;
        }

        skill.IsActive = false;
        await _repository.UpdateAsync(skill, cancellationToken);
        return skill;
    }
}
