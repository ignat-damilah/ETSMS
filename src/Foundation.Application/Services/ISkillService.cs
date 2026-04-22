using Foundation.Application.DTOs;

namespace Foundation.Application.Services;

public interface ISkillService
{
    /// <summary>
    /// Creates a new skill. Returns the created DTO and null error on success,
    /// or null DTO with an error message on validation failure.
    /// </summary>
    Task<(SkillDto? Skill, string? Error)> CreateAsync(
        CreateSkillRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a single skill by ID including parent metadata.
    /// Returns null when the skill does not exist.
    /// </summary>
    Task<SkillDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a filtered list of skills.
    /// Pass <paramref name="filterByParentSkillId"/> = true with a null
    /// <paramref name="parentSkillId"/> to retrieve only primary skills.
    /// </summary>
    Task<IEnumerable<SkillDto>> ListAsync(
        string? category,
        Guid? parentSkillId,
        bool filterByParentSkillId,
        bool? isActive,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing skill. Returns the updated DTO and null error on success,
    /// null DTO with null error when not found, or null DTO with an error message
    /// on validation failure.
    /// </summary>
    Task<(SkillDto? Skill, string? Error, bool NotFound)> UpdateAsync(
        Guid id,
        UpdateSkillRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft-deletes a skill by setting <c>IsActive = false</c>. The record is
    /// retained in the database. Returns (true, null, false) on success,
    /// (false, null, true) when the skill does not exist, or (false, error, false)
    /// when deletion is blocked by active children.
    /// </summary>
    Task<(bool Deleted, string? Error, bool NotFound)> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the direct secondary skills of a primary skill.
    /// Returns null skills with an error message when the addressed skill is itself secondary,
    /// or null skills with null error when the skill does not exist.
    /// </summary>
    Task<(IEnumerable<SkillDto>? Children, string? Error, bool NotFound)> GetChildrenAsync(
        Guid id,
        bool? isActive,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the parent primary skill of a secondary skill.
    /// Returns null when the skill does not exist, or an error when the skill is primary.
    /// </summary>
    Task<(SkillDto? Parent, string? Error, bool NotFound)> GetParentAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
