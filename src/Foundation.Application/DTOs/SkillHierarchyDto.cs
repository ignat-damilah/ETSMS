using System.Collections.Generic;

namespace Foundation.Application.DTOs;

public sealed class SkillHierarchyDto
{
    public SkillDto Current { get; set; } = new();
    public IEnumerable<SkillDto> Ancestors { get; set; } = new List<SkillDto>();
    public IEnumerable<SkillDto> Descendants { get; set; } = new List<SkillDto>();
}
