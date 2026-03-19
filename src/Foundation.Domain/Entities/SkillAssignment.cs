namespace Foundation.Domain.Entities;

public sealed class SkillAssignment
{
    public Guid Id { get; set; }
    public Guid SkillId { get; set; }
    public string AssignmentType { get; set; } = string.Empty;
    public Guid AssignmentId { get; set; }
    public Skill? Skill { get; set; }
}
