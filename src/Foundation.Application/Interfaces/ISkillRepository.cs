using Foundation.Domain.Entities;

namespace Foundation.Application.Interfaces;

/// <summary>
/// Persistence contract for the <see cref="Skill"/> aggregate.
/// </summary>
public interface ISkillRepository
{
    /// <summary>
    /// Returns all <em>active</em> skills (UI-facing default, <c>IsActive = true</c>).
    /// </summary>
    Task<IEnumerable<Skill>> GetAllActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all skills including inactive ones (admin / audit queries).
    /// </summary>
    Task<IEnumerable<Skill>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all active primary (top-level) skills, i.e. where <c>ParentSkillId IS NULL</c>.
    /// </summary>
    Task<IEnumerable<Skill>> GetPrimarySkillsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all active secondary skills that belong to the specified primary skill,
    /// i.e. where <c>ParentSkillId = <paramref name="primarySkillId"/></c>.
    /// </summary>
    Task<IEnumerable<Skill>> GetSecondarySkillsAsync(Guid primarySkillId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a skill by its identifier, or <c>null</c> if not found.
    /// Inactive skills are included so that referential integrity with future
    /// employee assessments is preserved (FR-6 / NFR-3).
    /// </summary>
    Task<Skill?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Persists a new skill to the store.</summary>
    Task AddAsync(Skill skill, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing skill (e.g. soft-delete via <c>IsActive = false</c>).</summary>
    Task UpdateAsync(Skill skill, CancellationToken cancellationToken = default);
}
