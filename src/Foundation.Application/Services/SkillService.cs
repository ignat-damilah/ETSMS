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

    public async Task<SkillDto> CreateAsync(SkillDto dto, CancellationToken cancellationToken = default)
    {
        var entity = new Skill
        {
            Id = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id,
            Name = dto.Name,
            Category = dto.Category,
            ParentSkillId = dto.ParentSkillId,
            IsActive = dto.IsActive,
            IsDeleted = dto.IsDeleted
        };

        ValidateHierarchy(entity);

        await _repository.AddAsync(entity, cancellationToken);

        return dto with { Id = entity.Id };
    }

    public async Task<SkillDto?> GetAsync(Guid id, bool includeDeleted = false, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, includeDeleted, cancellationToken);
        if (entity == null)
            return null;

        return Map(entity);
    }

    public async Task<SkillDto> UpdateAsync(Guid id, SkillDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, true, cancellationToken)
            ?? throw new KeyNotFoundException("Skill not found.");

        entity.Name = dto.Name;
        entity.Category = dto.Category;
        entity.ParentSkillId = dto.ParentSkillId;
        entity.IsActive = dto.IsActive;
        entity.IsDeleted = dto.IsDeleted;

        ValidateHierarchy(entity);

        await _repository.UpdateAsync(entity, cancellationToken);

        return Map(entity);
    }

    public async Task SoftDeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, true, cancellationToken)
                     ?? throw new KeyNotFoundException("Skill not found.");

        if (entity.Category == SkillCategory.Primary && await _repository.HasChildrenAsync(id, cancellationToken))
        {
            throw new InvalidOperationException("Cannot delete a primary skill that has secondary skills.");
        }

        await _repository.SoftDeleteAsync(entity, cancellationToken);
    }

    public async Task<IEnumerable<SkillDto>> ListAsync(bool? isActive = null, SkillCategory? category = null, Guid? parentSkillId = null, bool includeDeleted = false, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        var list = await _repository.ListAsync(isActive, category, parentSkillId, includeDeleted, cancellationToken);
        return list.Skip((page - 1) * pageSize)
                   .Take(pageSize)
                   .Select(Map);
    }

    public async Task<SkillDto?> GetParentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var parent = await _repository.GetParentAsync(id, cancellationToken);
        return parent == null ? null : Map(parent);
    }

    public async Task<IEnumerable<SkillDto>> GetChildrenAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var children = await _repository.GetChildrenAsync(id, cancellationToken);
        return children.Select(Map);
    }

    private static SkillDto Map(Skill skill)
    {
        return new SkillDto
        {
            Id = skill.Id,
            Name = skill.Name,
            Category = skill.Category,
            ParentSkillId = skill.ParentSkillId,
            IsActive = skill.IsActive,
            IsDeleted = skill.IsDeleted
        };
    }

    private static void ValidateHierarchy(Skill skill)
    {
        if (skill.Category == SkillCategory.Secondary && skill.ParentSkillId == null)
        {
            throw new ArgumentException("Secondary skills must reference a parent skill.");
        }

        if (skill.Category == SkillCategory.Primary && skill.ParentSkillId != null)
        {
            throw new ArgumentException("Primary skills cannot have a parent skill.");
        }
    }
}
