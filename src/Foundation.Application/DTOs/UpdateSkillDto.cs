using System;
using Foundation.Domain.Entities;

namespace Foundation.Application.DTOs;

public sealed class UpdateSkillDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public SkillCategory Category { get; set; }
    public Guid? ParentSkillId { get; set; }
    public bool IsActive { get; set; }
}