using System;

namespace Foundation.API.Requests;

public sealed class CreateSecondarySkillRequest
{
    public string Name { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public Guid PrimarySkillId { get; init; }
}
