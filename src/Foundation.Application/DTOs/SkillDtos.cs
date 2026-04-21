namespace Foundation.Application.DTOs;

public sealed record SkillDto(Guid Id, string Name, string Category, Guid? ParentSkillId, bool IsActive);

public sealed record CreatePrimarySkillRequest(string Name, string Category);

public sealed record CreateSecondarySkillRequest(string Name, string Category, Guid PrimarySkillId);
