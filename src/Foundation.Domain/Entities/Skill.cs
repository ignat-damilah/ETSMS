using System;
using System.Collections.Generic;

namespace Foundation.Domain.Entities;

public enum SkillCategory
{
    Primary,
    Secondary
}

public sealed class Skill
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public SkillCategory Category { get; set; }
    public Guid? ParentSkillId { get; set; }
    public bool IsActive { get; set; } = true;

    public Skill? ParentSkill { get; set; }
    public ICollection<Skill> ChildSkills { get; } = new List<Skill>();
}
