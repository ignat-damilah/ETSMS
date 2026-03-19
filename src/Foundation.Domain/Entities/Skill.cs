using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Foundation.Domain.Entities;

public sealed class Skill
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;
    public Guid? ParentSkillId { get; set; }

    public Skill? ParentSkill { get; set; }
    public ICollection<Skill> ChildSkills { get; } = new List<Skill>();
}
