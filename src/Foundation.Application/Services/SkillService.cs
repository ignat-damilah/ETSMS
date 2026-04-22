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

    // -------------------------------------------------------------------------
    // Create
    // -------------------------------------------------------------------------

    public async Task<(SkillDto? Skill, string? Error)> CreateAsync(
        CreateSkillRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return (null, "Name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Category))
        {
            return (null, "Category is required.");
        }

        if (request.ParentSkillId.HasValue)
        {
            var validationError = await ValidateParentAsync(request.ParentSkillId.Value, null, cancellationToken);
            if (validationError is not null)
            {
                return (null, validationError);
            }
        }

        var skill = new Skill
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Category = request.Category.Trim(),
            ParentSkillId = request.ParentSkillId,
            IsActive = true
        };

        await _repository.AddAsync(skill, cancellationToken);

        return (MapToDto(skill, parentName: null), null);
    }

    // -------------------------------------------------------------------------
    // Get by ID
    // -------------------------------------------------------------------------

    public async Task<SkillDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var skill = await _repository.GetByIdWithParentAsync(id, cancellationToken);
        if (skill is null)
        {
            return null;
        }

        return MapToDto(skill, skill.ParentSkill?.Name);
    }

    // -------------------------------------------------------------------------
    // List / Filter
    // -------------------------------------------------------------------------

    public async Task<IEnumerable<SkillDto>> ListAsync(
        string? category,
        Guid? parentSkillId,
        bool filterByParentSkillId,
        bool? isActive,
        CancellationToken cancellationToken = default)
    {
        var skills = await _repository.GetAllAsync(
            category,
            parentSkillId,
            filterByParentSkillId,
            isActive,
            cancellationToken);

        return skills.Select(s => MapToDto(s, s.ParentSkill?.Name));
    }

    // -------------------------------------------------------------------------
    // Update
    // -------------------------------------------------------------------------

    public async Task<(SkillDto? Skill, string? Error, bool NotFound)> UpdateAsync(
        Guid id,
        UpdateSkillRequest request,
        CancellationToken cancellationToken = default)
    {
        var skill = await _repository.GetByIdAsync(id, cancellationToken);
        if (skill is null)
        {
            return (null, null, true);
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return (null, "Name is required.", false);
        }

        if (string.IsNullOrWhiteSpace(request.Category))
        {
            return (null, "Category is required.", false);
        }

        if (request.ParentSkillId.HasValue)
        {
            // Cannot assign a skill as its own parent (direct cycle).
            if (request.ParentSkillId.Value == id)
            {
                return (null, "A skill cannot be its own parent.", false);
            }

            var validationError = await ValidateParentAsync(request.ParentSkillId.Value, id, cancellationToken);
            if (validationError is not null)
            {
                return (null, validationError, false);
            }
        }

        skill.Name = request.Name.Trim();
        skill.Category = request.Category.Trim();
        skill.ParentSkillId = request.ParentSkillId;
        skill.IsActive = request.IsActive;

        await _repository.UpdateAsync(skill, cancellationToken);

        // Reload to get parent name if applicable.
        var updated = await _repository.GetByIdWithParentAsync(id, cancellationToken);
        return (MapToDto(updated!, updated?.ParentSkill?.Name), null, false);
    }

    // -------------------------------------------------------------------------
    // Delete (soft)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Soft-deletes the skill by setting <c>IsActive = false</c>. The record is
    /// retained in the database. Deletion is blocked when the skill has active
    /// secondary children.
    /// </summary>
    public async Task<(bool Deleted, string? Error, bool NotFound)> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var skill = await _repository.GetByIdAsync(id, cancellationToken);
        if (skill is null)
        {
            return (false, null, true);
        }

        if (await _repository.HasActiveChildrenAsync(id, cancellationToken))
        {
            return (false,
                "Cannot deactivate this skill because it has active secondary skills. " +
                "Deactivate all active secondary skills before deactivating the parent.",
                false);
        }

        await _repository.DeleteAsync(skill, cancellationToken);
        return (true, null, false);
    }

    // -------------------------------------------------------------------------
    // Get children
    // -------------------------------------------------------------------------

    public async Task<(IEnumerable<SkillDto>? Children, string? Error, bool NotFound)> GetChildrenAsync(
        Guid id,
        bool? isActive,
        CancellationToken cancellationToken = default)
    {
        var skill = await _repository.GetByIdAsync(id, cancellationToken);
        if (skill is null)
        {
            return (null, null, true);
        }

        if (skill.ParentSkillId.HasValue)
        {
            return (null,
                "The specified skill is a secondary skill. Only primary skills can have children.",
                false);
        }

        var children = await _repository.GetChildrenAsync(id, isActive, cancellationToken);
        return (children.Select(s => MapToDto(s, skill.Name)), null, false);
    }

    // -------------------------------------------------------------------------
    // Get parent
    // -------------------------------------------------------------------------

    public async Task<(SkillDto? Parent, string? Error, bool NotFound)> GetParentAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var skill = await _repository.GetByIdWithParentAsync(id, cancellationToken);
        if (skill is null)
        {
            return (null, null, true);
        }

        if (!skill.ParentSkillId.HasValue)
        {
            return (null,
                "The specified skill is a primary skill and does not have a parent.",
                false);
        }

        if (skill.ParentSkill is null)
        {
            // Orphaned reference — parent record missing.
            return (null, null, true);
        }

        return (MapToDto(skill.ParentSkill, parentName: null), null, false);
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Validates a parent skill reference. Returns an error message string when
    /// the reference is invalid, or null when it is valid.
    /// </summary>
    private async Task<string?> ValidateParentAsync(
        Guid parentSkillId,
        Guid? skillBeingUpdatedId,
        CancellationToken cancellationToken)
    {
        var parent = await _repository.GetByIdAsync(parentSkillId, cancellationToken);

        if (parent is null)
        {
            return $"Parent skill with ID '{parentSkillId}' does not exist.";
        }

        // Parent must itself be a primary skill (no tertiary nesting).
        if (parent.ParentSkillId.HasValue)
        {
            return "The specified parent skill is a secondary skill. " +
                   "Only primary skills may be used as a parent.";
        }

        // Cycle check: if the parent is the skill being updated, reject.
        // (The self-parent case is already handled before calling this method.)
        if (skillBeingUpdatedId.HasValue && parent.Id == skillBeingUpdatedId.Value)
        {
            return "A skill cannot reference itself as a parent.";
        }

        return null;
    }

    private static SkillDto MapToDto(Skill skill, string? parentName) =>
        new()
        {
            Id = skill.Id,
            Name = skill.Name,
            Category = skill.Category,
            ParentSkillId = skill.ParentSkillId,
            ParentSkillName = parentName,
            IsActive = skill.IsActive
        };
}
