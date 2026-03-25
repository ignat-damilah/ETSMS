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

    public async Task<SkillDto> CreateAsync(CreateSkillRequest request, CancellationToken cancellationToken = default)
    {
        ValidateName(request.Name, nameof(request.Name));
        ValidateHierarchyRules(request.Category, request.ParentSkillId);

        if (request.ParentSkillId.HasValue)
        {
            await EnsureParentIsPrimaryAsync(request.ParentSkillId.Value, cancellationToken);
        }

        var skill = new Skill
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Category = request.Category,
            ParentSkillId = request.ParentSkillId,
            IsActive = true
        };

        await _repository.AddAsync(skill, cancellationToken);

        return Map(skill);
    }

    public async Task<SkillDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var skill = await _repository.GetByIdAsync(id, cancellationToken);
        return skill is null ? null : Map(skill);
    }

    public async Task<SkillDto> UpdateAsync(Guid id, UpdateSkillRequest request, CancellationToken cancellationToken = default)
    {
        var skill = await GetExistingSkillAsync(id, cancellationToken);

        if (request.Name is not null)
        {
            ValidateName(request.Name, nameof(request.Name));
            skill.Name = request.Name.Trim();
        }

        var desiredCategory = request.Category ?? skill.Category;
        var desiredParentId = DetermineParentId(skill, request, desiredCategory);

        ValidateHierarchyRules(desiredCategory, desiredParentId);

        if (desiredParentId.HasValue)
        {
            if (desiredParentId.Value == skill.Id)
            {
                throw new InvalidOperationException("A skill cannot reference itself as a parent.");
            }

            await EnsureParentIsPrimaryAsync(desiredParentId.Value, cancellationToken);
        }

        skill.Category = desiredCategory;
        skill.ParentSkillId = desiredCategory == SkillCategory.Primary ? null : desiredParentId;

        if (request.IsActive.HasValue)
        {
            skill.IsActive = request.IsActive.Value;
        }

        await _repository.UpdateAsync(skill, cancellationToken);

        return Map(skill);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var skill = await GetExistingSkillAsync(id, cancellationToken);

        if (skill.Category == SkillCategory.Primary && await _repository.HasActiveChildrenAsync(skill.Id, cancellationToken))
        {
            throw new InvalidOperationException("Cannot delete a primary skill with active secondary children.");
        }

        await _repository.DeleteAsync(skill, cancellationToken);
    }

    public async Task<IEnumerable<SkillDto>> ListAsync(SkillListFilter filter, CancellationToken cancellationToken = default)
    {
        var normalizedFilter = filter ?? new SkillListFilter();
        var skills = await _repository.FilterAsync(
            normalizedFilter.Category,
            normalizedFilter.ParentSkillId,
            normalizedFilter.IsActive,
            cancellationToken);

        return skills.Select(Map);
    }

    public async Task<IEnumerable<SkillDto>> GetChildrenAsync(Guid parentSkillId, CancellationToken cancellationToken = default)
    {
        await EnsureSkillExistsAsync(parentSkillId, cancellationToken);
        var children = await _repository.GetChildrenAsync(parentSkillId, cancellationToken);
        return children.Select(Map);
    }

    public async Task<SkillDto?> GetParentAsync(Guid skillId, CancellationToken cancellationToken = default)
    {
        var skill = await GetExistingSkillAsync(skillId, cancellationToken);
        if (skill.ParentSkill is null)
        {
            return null;
        }

        return Map(skill.ParentSkill);
    }

    private static void ValidateName(string? name, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name is required.", propertyName);
        }
    }

    private static void ValidateHierarchyRules(SkillCategory category, Guid? parentSkillId)
    {
        if (category == SkillCategory.Primary && parentSkillId is not null)
        {
            throw new InvalidOperationException("Primary skills cannot have a parent.");
        }

        if (category == SkillCategory.Secondary && parentSkillId is null)
        {
            throw new InvalidOperationException("Secondary skills must reference a primary parent.");
        }
    }

    private static Guid? DetermineParentId(Skill existing, UpdateSkillRequest request, SkillCategory desiredCategory)
    {
        if (request.ParentSkillId is not null)
        {
            return request.ParentSkillId;
        }

        if (request.ParentSkillId is null && request.Category is null && desiredCategory == SkillCategory.Secondary)
        {
            return existing.ParentSkillId;
        }

        if (request.ParentSkillId is null && request.Category is null && desiredCategory == SkillCategory.Primary)
        {
            return null;
        }

        if (request.ParentSkillId is null && request.Category is not null && desiredCategory == SkillCategory.Secondary)
        {
            return existing.ParentSkillId;
        }

        return null;
    }

    private async Task EnsureParentIsPrimaryAsync(Guid parentId, CancellationToken cancellationToken)
    {
        var parent = await _repository.GetByIdAsync(parentId, cancellationToken)
            ?? throw new KeyNotFoundException("Parent skill not found.");

        if (parent.Category != SkillCategory.Primary)
        {
            throw new InvalidOperationException("Parent skill must be primary.");
        }
    }

    private async Task<Skill> GetExistingSkillAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException("Skill not found.");
    }

    private async Task EnsureSkillExistsAsync(Guid id, CancellationToken cancellationToken)
    {
        if (!await _repository.ExistsAsync(id, cancellationToken))
        {
            throw new KeyNotFoundException("Skill not found.");
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
