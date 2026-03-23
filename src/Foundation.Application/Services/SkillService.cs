using Foundation.Application.DTOs;
using Foundation.Application.Interfaces;
using Foundation.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Services;

public sealed class SkillService : ISkillService
{
    private readonly ISkillRepository _repository;
    private readonly ILogger<SkillService> _logger;

    public SkillService(ISkillRepository repository, ILogger<SkillService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IEnumerable<SkillDto>> ListAsync(Guid? parentSkillId = null, CancellationToken cancellationToken = default)
    {
        var skills = await _repository.GetAllAsync(parentSkillId, cancellationToken);
        return skills.Select(ToDto).ToList();
    }

    public async Task<SkillDto?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var skill = await _repository.GetByIdAsync(id, cancellationToken);
        return skill is null ? null : ToDto(skill);
    }

    public async Task<SkillDto> CreateAsync(SkillDto skill, string userId, CancellationToken cancellationToken = default)
    {
        var normalizedName = NormalizeName(skill.Name);
        await ValidateHierarchyAsync(skill.ParentSkillId, cancellationToken);
        await EnsureNameIsUniqueAsync(normalizedName, skill.ParentSkillId, cancellationToken);

        var entity = new Skill
        {
            Id = Guid.NewGuid(),
            Name = normalizedName,
            Category = skill.Category,
            ParentSkillId = skill.ParentSkillId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(entity, cancellationToken);
        LogAction("Create", entity.Id, userId);
        return ToDto(entity);
    }

    public async Task<SkillDto?> UpdateAsync(Guid id, SkillDto skill, string userId, CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            return null;
        }

        var normalizedName = NormalizeName(skill.Name);
        await ValidateHierarchyAsync(skill.ParentSkillId, cancellationToken);
        if (!string.Equals(existing.Name, normalizedName, StringComparison.OrdinalIgnoreCase) || existing.ParentSkillId != skill.ParentSkillId)
        {
            await EnsureNameIsUniqueAsync(normalizedName, skill.ParentSkillId, cancellationToken, existing.Id);
        }

        existing.Name = normalizedName;
        existing.Category = skill.Category;
        existing.ParentSkillId = skill.ParentSkillId;
        existing.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(existing, cancellationToken);
        LogAction("Update", existing.Id, userId);
        return ToDto(existing);
    }

    public async Task<bool> DeleteAsync(Guid id, string userId, CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            return false;
        }

        if (existing.IsPrimary)
        {
            var secondaries = await _repository.GetAllAsync(existing.Id, cancellationToken);
            if (secondaries.Any())
            {
                throw new InvalidOperationException("Cannot delete a primary skill that still has secondary skills.");
            }
        }

        await _repository.DeleteAsync(existing, cancellationToken);
        LogAction("Delete", existing.Id, userId);
        return true;
    }

    public async Task<IEnumerable<SkillDto>> ListSecondariesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var secondaries = await _repository.GetAllAsync(id, cancellationToken);
        return secondaries.Select(ToDto).ToList();
    }

    private static SkillDto ToDto(Skill skill) =>
        new()
        {
            Id = skill.Id,
            Name = skill.Name,
            Category = skill.Category,
            ParentSkillId = skill.ParentSkillId,
            CreatedAt = skill.CreatedAt,
            UpdatedAt = skill.UpdatedAt
        };

    private async Task ValidateHierarchyAsync(Guid? parentSkillId, CancellationToken cancellationToken)
    {
        if (parentSkillId is null)
        {
            return;
        }

        var parent = await _repository.GetByIdAsync(parentSkillId.Value, cancellationToken);
        if (parent is null || !parent.IsPrimary)
        {
            throw new InvalidOperationException("Parent skill must be an existing primary skill.");
        }
    }

    private static string NormalizeName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Skill name must be provided.", nameof(name));
        }

        var trimmed = name.Trim();
        if (trimmed.Length < 3 || trimmed.Length > 100)
        {
            throw new ArgumentException("Skill name must contain between 3 and 100 characters.", nameof(name));
        }

        return trimmed;
    }

    private async Task EnsureNameIsUniqueAsync(string normalizedName, Guid? parentSkillId, CancellationToken cancellationToken, Guid? excludingId = null)
    {
        if (await _repository.ExistsByNameAsync(normalizedName, parentSkillId, excludingId, cancellationToken))
        {
            throw new InvalidOperationException("A skill with the same name already exists at this level.");
        }
    }

    private void LogAction(string action, Guid skillId, string userId)
    {
        _logger.LogInformation("Skill {Action} performed for {SkillId} by {UserId} at {Timestamp}.", action, skillId, userId, DateTime.UtcNow);
    }
}
