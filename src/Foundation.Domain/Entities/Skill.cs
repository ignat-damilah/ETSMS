using Foundation.Domain.Enums;

namespace Foundation.Domain.Entities;

/// <summary>
/// Represents a skill in the two-level taxonomy hierarchy.
/// A skill with no parent is a <em>primary</em> (top-level) skill.
/// A skill with a parent reference is a <em>secondary</em> skill.
/// The hierarchy is strictly two levels deep — a secondary skill may not itself
/// be used as the parent of another skill (NFR-4).
/// </summary>
public sealed class Skill
{
    /// <summary>System-generated primary key.</summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Display name of the skill.
    /// Must be unique within the same <see cref="Category"/> and <see cref="ParentSkillId"/> scope.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Domain area that classifies the skill.</summary>
    public SkillCategory Category { get; set; }

    /// <summary>
    /// Foreign key to the parent <see cref="Skill"/>.
    /// <c>null</c> for primary (top-level) skills.
    /// Must reference the <see cref="Id"/> of a primary skill for secondary skills.
    /// </summary>
    public Guid? ParentSkillId { get; set; }

    /// <summary>
    /// Whether the skill is currently active.
    /// Defaults to <c>true</c> on creation.
    /// Set to <c>false</c> to deprecate a skill (soft delete — records are never hard-deleted).
    /// </summary>
    public bool IsActive { get; set; } = true;

    // -----------------------------------------------------------------
    // Navigation properties
    // -----------------------------------------------------------------

    /// <summary>The parent primary skill; <c>null</c> when this is itself a primary skill.</summary>
    public Skill? ParentSkill { get; set; }

    /// <summary>The secondary skills that belong to this primary skill.</summary>
    public ICollection<Skill> ChildSkills { get; set; } = new List<Skill>();

    // -----------------------------------------------------------------
    // Domain behaviour
    // -----------------------------------------------------------------

    /// <summary>
    /// Returns <c>true</c> when this skill is a primary (top-level) skill,
    /// i.e. it has no parent.
    /// </summary>
    public bool IsPrimary => ParentSkillId is null;

    /// <summary>
    /// Returns <c>true</c> when this skill is a secondary skill,
    /// i.e. it has a parent reference.
    /// </summary>
    public bool IsSecondary => ParentSkillId is not null;

    /// <summary>
    /// Validates that the proposed <paramref name="parentSkill"/> may be used as the parent of
    /// this skill, enforcing the two-level hierarchy constraint (NFR-4).
    /// </summary>
    /// <param name="parentSkill">The candidate parent skill.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <paramref name="parentSkill"/> is itself a secondary skill, which would
    /// create a hierarchy deeper than two levels.
    /// </exception>
    public void SetParent(Skill parentSkill)
    {
        ArgumentNullException.ThrowIfNull(parentSkill);

        if (parentSkill.IsSecondary)
        {
            throw new InvalidOperationException(
                $"Cannot set skill '{parentSkill.Name}' (Id: {parentSkill.Id}) as a parent " +
                "because it is already a secondary skill. The skill hierarchy is strictly " +
                "two levels deep (primary → secondary only).");
        }

        ParentSkillId = parentSkill.Id;
        ParentSkill = parentSkill;
    }

    /// <summary>
    /// Deprecates the skill by setting <see cref="IsActive"/> to <c>false</c>.
    /// The record is fully preserved; no data is deleted.
    /// </summary>
    public void Deprecate()
    {
        IsActive = false;
    }
}
