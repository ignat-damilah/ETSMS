using System;
using Foundation.Domain.Entities;

namespace Foundation.Application.DTOs;

public sealed class CreateSkillDto
{
    public string Name { get; set; } = string.Empty;
    public SkillCategory Category { get; set; }
    public Guid? ParentSkillId { get; set; }
}