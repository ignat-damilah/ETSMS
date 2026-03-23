using System;
using Foundation.Domain.Entities;

namespace Foundation.Application.DTOs;

public sealed class SkillDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public SkillCategory Category { get; set; }
    public Guid? ParentSkillId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
