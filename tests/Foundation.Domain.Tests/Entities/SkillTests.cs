using Foundation.Domain.Entities;
using Foundation.Domain.Enums;

namespace Foundation.Domain.Tests.Entities;

/// <summary>
/// Unit tests for the <see cref="Skill"/> entity covering all five acceptance
/// criteria (AC-1 through AC-5) and the two-level hierarchy enforcement (NFR-4).
/// </summary>
public sealed class SkillTests
{
    // -----------------------------------------------------------------
    // AC-1 — Skill entity has the required properties
    // -----------------------------------------------------------------

    [Fact]
    public void Skill_HasRequiredProperties()
    {
        // Arrange & Act
        var skill = new Skill
        {
            Id = Guid.NewGuid(),
            Name = "JavaScript",
            Category = SkillCategory.Frontend,
            IsActive = true
        };

        // Assert — all five properties from FR-1 are present and accessible
        Assert.IsType<Guid>(skill.Id);
        Assert.Equal("JavaScript", skill.Name);
        Assert.Equal(SkillCategory.Frontend, skill.Category);
        Assert.Null(skill.ParentSkillId);
        Assert.True(skill.IsActive);
    }

    [Fact]
    public void Skill_IsActive_DefaultsToTrue()
    {
        // Arrange & Act
        var skill = new Skill();

        // Assert — A-5: IsActive defaults to true at the application level
        Assert.True(skill.IsActive);
    }

    // -----------------------------------------------------------------
    // AC-2 — Primary skill is saved with ParentSkillId = null
    // -----------------------------------------------------------------

    [Fact]
    public void PrimarySkill_ParentSkillId_IsNull()
    {
        // Arrange
        var primarySkill = new Skill
        {
            Id = Guid.NewGuid(),
            Name = ".NET Core",
            Category = SkillCategory.Backend
        };

        // Assert
        Assert.Null(primarySkill.ParentSkillId);
        Assert.True(primarySkill.IsPrimary);
        Assert.False(primarySkill.IsSecondary);
    }

    // -----------------------------------------------------------------
    // AC-3 — Secondary skill ParentSkillId references a valid primary skill
    // -----------------------------------------------------------------

    [Fact]
    public void SecondarySkill_ParentSkillId_ReferencesValidPrimarySkill()
    {
        // Arrange
        var primarySkill = new Skill
        {
            Id = Guid.NewGuid(),
            Name = "JavaScript",
            Category = SkillCategory.Frontend
        };

        var secondarySkill = new Skill
        {
            Id = Guid.NewGuid(),
            Name = "React",
            Category = SkillCategory.Frontend
        };

        // Act
        secondarySkill.SetParent(primarySkill);

        // Assert
        Assert.Equal(primarySkill.Id, secondarySkill.ParentSkillId);
        Assert.False(secondarySkill.IsPrimary);
        Assert.True(secondarySkill.IsSecondary);
    }

    [Fact]
    public void SetParent_WithNullParent_ThrowsArgumentNullException()
    {
        // Arrange
        var skill = new Skill { Id = Guid.NewGuid(), Name = "React", Category = SkillCategory.Frontend };

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => skill.SetParent(null!));
    }

    // -----------------------------------------------------------------
    // NFR-4 — Two-level hierarchy constraint: secondary cannot be a parent
    // -----------------------------------------------------------------

    [Fact]
    public void SetParent_WithSecondarySkillAsParent_ThrowsInvalidOperationException()
    {
        // Arrange — create a secondary skill (already has a parent)
        var primarySkill = new Skill
        {
            Id = Guid.NewGuid(),
            Name = "JavaScript",
            Category = SkillCategory.Frontend
        };

        var secondarySkill = new Skill
        {
            Id = Guid.NewGuid(),
            Name = "React",
            Category = SkillCategory.Frontend
        };
        secondarySkill.SetParent(primarySkill);

        var thirdLevelSkill = new Skill
        {
            Id = Guid.NewGuid(),
            Name = "React Hooks",
            Category = SkillCategory.Frontend
        };

        // Act & Assert — attempting to use a secondary skill as a parent must fail (NFR-4)
        var exception = Assert.Throws<InvalidOperationException>(() => thirdLevelSkill.SetParent(secondarySkill));
        Assert.Contains("two levels deep", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    // -----------------------------------------------------------------
    // AC-4 — Query: secondary skills under a given primary skill
    // -----------------------------------------------------------------

    [Fact]
    public void PrimarySkill_ChildSkills_ContainsAllSecondarySkills()
    {
        // Arrange
        var primarySkill = new Skill
        {
            Id = Guid.NewGuid(),
            Name = "Cloud",
            Category = SkillCategory.Cloud
        };

        var azure = new Skill { Id = Guid.NewGuid(), Name = "Azure", Category = SkillCategory.Cloud };
        var aws = new Skill { Id = Guid.NewGuid(), Name = "AWS", Category = SkillCategory.Cloud };
        var gcp = new Skill { Id = Guid.NewGuid(), Name = "GCP", Category = SkillCategory.Cloud };

        azure.SetParent(primarySkill);
        aws.SetParent(primarySkill);
        gcp.SetParent(primarySkill);

        // Simulate what would be returned by the repository (all with matching ParentSkillId)
        var secondarySkills = new[] { azure, aws, gcp }
            .Where(s => s.ParentSkillId == primarySkill.Id)
            .ToList();

        // Assert — all three secondary skills are returned under the primary
        Assert.Equal(3, secondarySkills.Count);
        Assert.Contains(secondarySkills, s => s.Name == "Azure");
        Assert.Contains(secondarySkills, s => s.Name == "AWS");
        Assert.Contains(secondarySkills, s => s.Name == "GCP");
    }

    // -----------------------------------------------------------------
    // AC-5 — Deprecation: IsActive set to false, record fully preserved
    // -----------------------------------------------------------------

    [Fact]
    public void Deprecate_SetsIsActiveToFalse_AndPreservesAllProperties()
    {
        // Arrange
        var skillId = Guid.NewGuid();
        var skill = new Skill
        {
            Id = skillId,
            Name = "GitHub Copilot",
            Category = SkillCategory.AITools,
            IsActive = true
        };

        // Act
        skill.Deprecate();

        // Assert — IsActive is false but all other properties are intact (FR-6 / NFR-2)
        Assert.False(skill.IsActive);
        Assert.Equal(skillId, skill.Id);
        Assert.Equal("GitHub Copilot", skill.Name);
        Assert.Equal(SkillCategory.AITools, skill.Category);
        Assert.Null(skill.ParentSkillId);
    }

    [Fact]
    public void Deprecate_OnSecondarySkill_PreservesParentSkillId()
    {
        // Arrange
        var primarySkill = new Skill
        {
            Id = Guid.NewGuid(),
            Name = "AI Tools",
            Category = SkillCategory.AITools
        };

        var secondarySkill = new Skill
        {
            Id = Guid.NewGuid(),
            Name = "Cursor",
            Category = SkillCategory.AITools
        };
        secondarySkill.SetParent(primarySkill);

        // Act
        secondarySkill.Deprecate();

        // Assert — ParentSkillId is retained even after deprecation (NFR-3)
        Assert.False(secondarySkill.IsActive);
        Assert.Equal(primarySkill.Id, secondarySkill.ParentSkillId);
        Assert.Equal("Cursor", secondarySkill.Name);
    }

    // -----------------------------------------------------------------
    // Category coverage — all six SkillCategory values are valid
    // -----------------------------------------------------------------

    [Theory]
    [InlineData(SkillCategory.Frontend)]
    [InlineData(SkillCategory.Backend)]
    [InlineData(SkillCategory.AITools)]
    [InlineData(SkillCategory.Cloud)]
    [InlineData(SkillCategory.BusinessDomain)]
    [InlineData(SkillCategory.Certification)]
    public void Skill_SupportsAllSkillCategories(SkillCategory category)
    {
        // Arrange & Act
        var skill = new Skill
        {
            Id = Guid.NewGuid(),
            Name = $"Test Skill ({category})",
            Category = category
        };

        // Assert
        Assert.Equal(category, skill.Category);
    }
}
