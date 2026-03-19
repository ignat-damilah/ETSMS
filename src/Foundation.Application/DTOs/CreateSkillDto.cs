using System;

namespace Foundation.Application.DTOs;

public sealed class CreateSkillDto
{
    public string Name { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public Guid? ParentSkillId { get; init; }
}
