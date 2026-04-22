namespace Foundation.Application.DTOs;

public sealed class SkillSummaryDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
}

public sealed class SkillDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public Guid? ParentSkillId { get; init; }
    public SkillSummaryDto? ParentSkill { get; init; }
    public bool IsActive { get; init; }
    public bool IsDeleted { get; init; }
}

public sealed class CreateSkillRequest
{
    public string Name { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public Guid? ParentSkillId { get; init; }
}

public sealed class UpdateSkillRequest
{
    public string Name { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public Guid? ParentSkillId { get; init; }
    public bool IsActive { get; init; } = true;
}

public sealed class SkillFilterParams
{
    public string? Category { get; init; }
    public Guid? ParentSkillId { get; init; }
    public bool? IsActive { get; init; }
    public bool? IsDeleted { get; init; }
}
