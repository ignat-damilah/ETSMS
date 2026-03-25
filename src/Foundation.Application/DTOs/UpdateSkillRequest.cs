using Foundation.Domain.Entities;

namespace Foundation.Application.DTOs;

public sealed class UpdateSkillRequest
{
    public string? Name { get; init; }
    public SkillCategory? Category { get; init; }
    public Guid? ParentSkillId { get; init; }
    public bool? IsActive { get; init; }
}
