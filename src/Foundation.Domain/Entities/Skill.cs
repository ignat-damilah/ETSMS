using System;
using System.Collections.Generic;

namespace Foundation.Domain.Entities;

public sealed class Skill
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public Guid? ParentSkillId { get; set; }
    public bool IsActive { get; set; } = true;

    public Skill? ParentSkill { get; set; }
    public ICollection<Skill> SecondarySkills { get; set; } = new List<Skill>();
}
