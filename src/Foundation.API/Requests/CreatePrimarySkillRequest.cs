namespace Foundation.API.Requests;

public sealed class CreatePrimarySkillRequest
{
    public string Name { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
}
