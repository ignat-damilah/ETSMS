using Foundation.Application.DTOs;
using Foundation.Application.Interfaces;
using Foundation.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Foundation.Application.Services;

public sealed class SkillService : ISkillService
{
    private readonly ISkillRepository _repository;

    public SkillService(ISkillRepository repository)
    {
        _repository = repository;
    }

    public async Task<SkillDto> CreateAsync(SkillCreateDto request, CancellationToken cancellationToken = default)
    {
        ValidateName(request.Name);
        ValidateCategory(request.Category);

        var normalizedName = request.Name.Trim();

        if (request.Category == SkillCategory.Secondary)
        {
            if (request.ParentSkillId is null)
            {
                throw new ArgumentException("Secondary skills must reference a parent.", nameof(request.ParentSkillId));
            }

            await EnsurePrimaryParentIsValidAsync(request.ParentSkillId.Value, cancellationToken);
        }

        var existing = await _repository.ListAsync(cancellationToken);
        if (existing.Any(s => s.Name.Equals(normalizedName, StringComparison.OrdinalIgnoreCase) && s.Category == request.Category))
        {
            throw new InvalidOperationException("Skill already exists in the same category.");
        }

        var skill = new Skill
        {
            Id = Guid.NewGuid(),
            Name = normalizedName,
            Category = request.Category.Value,
            ParentSkillId = request.Category == SkillCategory.Secondary ? request.ParentSkillId : null,
            IsActive = true
        };

        await _repository.AddAsync(skill, cancellationToken);
        return Map(skill);
    }

    public async Task<SkillDto?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var skill = await _repository.GetByIdAsync(id, cancellationToken);
        return skill is null ? null : Map(skill);
    }

    public async Task<IEnumerable<SkillDto>> ListAsync(SkillCategory? category, Guid? parentSkillId, bool? isActive, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var pageIndex = Math.Max(page, 1);
        var size = Math.Clamp(pageSize, 1, 100);
        var skills = await _repository.QueryAsync(category, parentSkillId, isActive, pageIndex, size, cancellationToken);
        return skills.Select(Map);
    }

    public async Task<SkillDto> UpdateAsync(Guid id, SkillUpdateDto request, CancellationToken cancellationToken = default)
    {
        var skill = await GetSkillOrThrowAsync(id, cancellationToken);

        ValidateName(request.Name);
        ValidateCategory(request.Category);

        if (skill.Category != request.Category)
        {
            throw new ArgumentException("Cannot change skill category.", nameof(request.Category));
        }

        var normalizedName = request.Name.Trim();
        var existing = await _repository.ListAsync(cancellationToken);
        if (existing.Any(s => s.Id != id && s.Category == skill.Category && s.Name.Equals(normalizedName, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("Skill already exists in the same category.");
        }

        if (request.Category == SkillCategory.Secondary)
        {
            if (request.ParentSkillId is null)
            {
                throw new ArgumentException("Secondary skills must reference a parent.", nameof(request.ParentSkillId));
            }

            await EnsurePrimaryParentIsValidAsync(request.ParentSkillId.Value, cancellationToken);
        }
        else
        {
            request.ParentSkillId = null;
        }

        skill.Name = normalizedName;
        skill.IsActive = request.IsActive;
        skill.ParentSkillId = request.Category == SkillCategory.Secondary ? request.ParentSkillId : null;

        await _repository.UpdateAsync(skill, cancellationToken);
        return Map(skill);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var skill = await GetSkillOrThrowAsync(id, cancellationToken);

        if (skill.Category == SkillCategory.Primary)
        {
            var children = skill.Children.Where(c => c.IsActive);
            if (children.Any())
            {
                throw new InvalidOperationException("Cannot delete a primary skill with active secondaries.");
            }
        }

        await _repository.DeleteAsync(skill, cancellationToken);
    }

    public async Task<IEnumerable<SkillDto>> GetChildrenAsync(Guid parentId, CancellationToken cancellationToken = default)
    {
        await GetSkillOrThrowAsync(parentId, cancellationToken);
        var children = await _repository.QueryAsync(SkillCategory.Secondary, parentId, null, 1, int.MaxValue, cancellationToken);
        return children.Select(Map);
    }

    public async Task<SkillDto?> GetParentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var skill = await GetSkillOrThrowAsync(id, cancellationToken);
        if (skill.ParentSkillId is null)
        {
            return null;
        }

        var parent = await _repository.GetByIdAsync(skill.ParentSkillId.Value, cancellationToken);
        return parent is null ? null : Map(parent);
    }

    private static void ValidateName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name is required.", nameof(name));
        }
    }

    private static void ValidateCategory(SkillCategory? category)
    {
        if (category is null)
        {
            throw new ArgumentException("Category is required.", nameof(category));
        }
    }

    private async Task<Skill> GetSkillOrThrowAsync(Guid id, CancellationToken cancellationToken)
    {
        var skill = await _repository.GetByIdAsync(id, cancellationToken);
        return skill ?? throw new KeyNotFoundException("Skill not found.");
    }

    private async Task EnsurePrimaryParentIsValidAsync(Guid parentSkillId, CancellationToken cancellationToken)
    {
        var parent = await _repository.GetByIdAsync(parentSkillId, cancellationToken);
        if (parent is null || parent.Category != SkillCategory.Primary || !parent.IsActive)
        {
            throw new ArgumentException("Parent skill must be an active primary skill.", nameof(parentSkillId));
        }
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
