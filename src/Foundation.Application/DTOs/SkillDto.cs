using Foundation.Domain.Entities;

namespace Foundation.Application.DTOs;

public sealed class SkillDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public SkillCategory Category { get; init; }
    public Guid? ParentSkillId { get; init; }
    public bool IsActive { get; init; }
}
