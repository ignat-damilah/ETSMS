using System;

namespace Foundation.Domain.Entities;

public sealed class Skill
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public Guid? ParentSkillId { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;

    // Navigation properties
    public Skill? ParentSkill { get; set; }
    public ICollection<Skill> ChildSkills { get; set; } = new List<Skill>();
}
