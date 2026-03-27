using System.ComponentModel.DataAnnotations;
using Foundation.Domain.Entities;

namespace Foundation.Application.DTOs;

public sealed class SkillUpdateDto
{
    [Required]
    public string Name { get; init; } = string.Empty;

    [Required]
    public SkillCategory? Category { get; init; }

    public Guid? ParentSkillId { get; init; }
    public bool IsActive { get; init; } = true;
}
