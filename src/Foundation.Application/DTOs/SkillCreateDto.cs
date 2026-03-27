using System.ComponentModel.DataAnnotations;
using Foundation.Domain.Entities;

namespace Foundation.Application.DTOs;

public sealed class SkillCreateDto
{
    [Required]
    public string Name { get; init; } = string.Empty;

    [Required]
    public SkillCategory? Category { get; init; }

    public Guid? ParentSkillId { get; init; }
}
