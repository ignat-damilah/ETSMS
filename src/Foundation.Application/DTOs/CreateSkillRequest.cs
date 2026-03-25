using Foundation.Domain.Entities;

namespace Foundation.Application.DTOs;

public sealed class CreateSkillRequest
{
    public string Name { get; init; } = string.Empty;
    public SkillCategory Category { get; init; }
    public Guid? ParentSkillId { get; init; }
}
