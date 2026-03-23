using System;
using System.Collections.Generic;

namespace Foundation.Domain.Entities;

public enum SkillCategory
{
    Unknown = 0,
    Engineering,
    Product,
    DataScience,
    Design,
    Leadership,
    Operations,
    Research
}

public sealed class Skill
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public SkillCategory Category { get; set; }
    public Guid? ParentSkillId { get; set; }
    public Skill? ParentSkill { get; set; }
    public ICollection<Skill> SecondarySkills { get; } = new List<Skill>();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public bool IsPrimary => ParentSkillId is null;
}
