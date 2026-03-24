using System;

namespace Foundation.Application.DTOs;

public sealed class SkillDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public Guid? ParentSkillId { get; set; }
    public bool IsActive { get; set; }
}
