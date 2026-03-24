using System;

namespace Foundation.Application.DTOs;

public sealed class SkillCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public Guid? ParentSkillId { get; set; }
}
