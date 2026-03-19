namespace Foundation.Application.DTOs;

public sealed class SkillTreeNodeDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public bool IsDeprecated { get; init; }
    public Guid? ParentSkillId { get; init; }
    public ICollection<SkillTreeNodeDto> Children { get; init; } = Array.Empty<SkillTreeNodeDto>();
}

public sealed class SkillCreateDto
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public Guid? ParentSkillId { get; init; }
}

public sealed class SkillUpdateDto
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public Guid? ParentSkillId { get; init; }
}
