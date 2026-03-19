using System;
using System.Collections.Generic;

namespace Foundation.Domain.Entities;

public sealed class Skill
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public Guid? ParentSkillId { get; set; }
    public Skill? ParentSkill { get; set; }
    public ICollection<Skill> SecondarySkills { get; set; } = new List<Skill>();
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
