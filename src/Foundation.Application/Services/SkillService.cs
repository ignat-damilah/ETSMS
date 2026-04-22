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

    public async Task<(SkillDto Skill, string Location)> CreateAsync(
        CreateSkillRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Name is required.", nameof(request));

        if (string.IsNullOrWhiteSpace(request.Category))
            throw new ArgumentException("Category is required.", nameof(request));

        if (request.ParentSkillId.HasValue)
            await ValidateParentAsync(request.ParentSkillId.Value, null, cancellationToken);

        var skill = new Skill
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Category = request.Category.Trim(),
            ParentSkillId = request.ParentSkillId,
            IsActive = true,
            IsDeleted = false
        };

        await _repository.AddAsync(skill, cancellationToken);

        var created = await _repository.GetByIdWithParentAsync(skill.Id, cancellationToken)
            ?? skill;

        return (MapToDto(created), $"/api/skills/{skill.Id}");
    }

    public async Task<SkillDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var skill = await _repository.GetByIdWithParentAsync(id, cancellationToken);

        if (skill is null || skill.IsDeleted)
            return null;

        return MapToDto(skill);
    }

    public async Task<SkillDto> UpdateAsync(
        Guid id,
        UpdateSkillRequest request,
        CancellationToken cancellationToken = default)
    {
        var skill = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Skill with id '{id}' was not found.");

        if (skill.IsDeleted)
            throw new InvalidOperationException("Cannot update a skill that has been deleted.");

        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Name is required.", nameof(request));

        if (string.IsNullOrWhiteSpace(request.Category))
            throw new ArgumentException("Category is required.", nameof(request));

        if (request.ParentSkillId.HasValue)
            await ValidateParentAsync(request.ParentSkillId.Value, id, cancellationToken);

        skill.Name = request.Name.Trim();
        skill.Category = request.Category.Trim();
        skill.ParentSkillId = request.ParentSkillId;
        skill.IsActive = request.IsActive;

        await _repository.UpdateAsync(skill, cancellationToken);

        var updated = await _repository.GetByIdWithParentAsync(id, cancellationToken) ?? skill;

        return MapToDto(updated);
    }

    public async Task SoftDeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var skill = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Skill with id '{id}' was not found.");

        var hasActiveChildren = await _repository.HasActiveUndeletedChildrenAsync(id, cancellationToken);

        if (hasActiveChildren)
            throw new InvalidOperationException(
                "Cannot delete this skill because it has active child skills. " +
                "Please deactivate or delete all child skills before deleting the parent.");

        skill.IsDeleted = true;

        await _repository.UpdateAsync(skill, cancellationToken);
    }

    public async Task<IEnumerable<SkillDto>> ListAsync(
        SkillFilterParams filters,
        CancellationToken cancellationToken = default)
    {
        var skills = await _repository.GetAllAsync(filters, cancellationToken);
        return skills.Select(MapToDto);
    }

    public async Task<IEnumerable<SkillDto>> GetChildrenAsync(
        Guid id,
        bool? isActive,
        bool? isDeleted,
        CancellationToken cancellationToken = default)
    {
        var skill = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Skill with id '{id}' was not found.");

        if (skill.ParentSkillId.HasValue)
            throw new InvalidOperationException(
                "The specified skill is a secondary skill and cannot have children. " +
                "Only primary skills (those with no ParentSkillId) may have children.");

        var children = await _repository.GetChildrenAsync(id, isActive, isDeleted, cancellationToken);
        return children.Select(MapToDto);
    }

    public async Task<SkillDto?> GetParentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var skill = await _repository.GetByIdWithParentAsync(id, cancellationToken);

        if (skill is null)
            return null;

        if (!skill.ParentSkillId.HasValue || skill.ParentSkill is null)
            return null;

        var parent = skill.ParentSkill;

        if (parent.IsDeleted)
            return null;

        return MapToDto(parent);
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    private async Task ValidateParentAsync(
        Guid parentSkillId,
        Guid? skillBeingUpdatedId,
        CancellationToken cancellationToken)
    {
        var parent = await _repository.GetByIdAsync(parentSkillId, cancellationToken);

        if (parent is null)
            throw new ArgumentException(
                $"Parent skill with id '{parentSkillId}' does not exist.");

        if (parent.IsDeleted)
            throw new ArgumentException(
                $"Parent skill with id '{parentSkillId}' has been deleted and cannot be assigned as a parent.");

        if (parent.ParentSkillId.HasValue)
            throw new ArgumentException(
                $"Parent skill with id '{parentSkillId}' is a secondary skill. " +
                "Only primary skills (those with no ParentSkillId) may be assigned as a parent.");

        // Guard against self-reference / cycles
        if (skillBeingUpdatedId.HasValue && parentSkillId == skillBeingUpdatedId.Value)
            throw new ArgumentException("A skill cannot be its own parent.");
    }

    private static SkillDto MapToDto(Skill skill)
    {
        SkillSummaryDto? parentSummary = null;

        if (skill.ParentSkill is not null)
        {
            parentSummary = new SkillSummaryDto
            {
                Id = skill.ParentSkill.Id,
                Name = skill.ParentSkill.Name,
                Category = skill.ParentSkill.Category
            };
        }

        return new SkillDto
        {
            Id = skill.Id,
            Name = skill.Name,
            Category = skill.Category,
            ParentSkillId = skill.ParentSkillId,
            ParentSkill = parentSummary,
            IsActive = skill.IsActive,
            IsDeleted = skill.IsDeleted
        };
    }
}
