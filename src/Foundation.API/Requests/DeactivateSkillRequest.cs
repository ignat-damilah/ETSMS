using System;

namespace Foundation.API.Requests;

public sealed class DeactivateSkillRequest
{
    public Guid SkillId { get; init; }
}
