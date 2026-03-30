using System.ComponentModel.DataAnnotations.Schema;

namespace Foundation.Domain.Entities;

public sealed class Skill
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public SkillCategory Category { get; set; }
    public Guid? ParentSkillId { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }

    [ForeignKey("ParentSkillId")]
    public Skill? ParentSkill { get; set; }
    public ICollection<Skill> Children { get; set; } = new List<Skill>();
}
