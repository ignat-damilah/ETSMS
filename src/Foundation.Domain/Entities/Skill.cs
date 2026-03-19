using System;
using System.Collections.Generic;

namespace Foundation.Domain.Entities;

public sealed class Skill
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid? ParentSkillId { get; set; }
    public Skill? ParentSkill { get; set; }
    public List<Skill> ChildSkills { get; } = new();
    public bool IsDeprecated { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<SkillAssignment> Assignments { get; } = new();
}
