namespace Foundation.Domain.Enums;

/// <summary>
/// Classifies the domain area of a skill.
/// Both technical and business domain categories are supported so that company
/// leadership can respond to client proposals across all skill types.
/// </summary>
public enum SkillCategory
{
    /// <summary>Frontend technologies such as JavaScript, React, Angular, and Vue.</summary>
    Frontend = 1,

    /// <summary>Backend technologies such as .NET Core, Java, Spring Boot, and Node.js.</summary>
    Backend = 2,

    /// <summary>AI-assisted tooling such as GitHub Copilot, Cursor, Codex, and Devlin.</summary>
    AITools = 3,

    /// <summary>Cloud platforms such as Azure, AWS, and GCP.</summary>
    Cloud = 4,

    /// <summary>Business domain knowledge such as HR, Finance, Rostering, and Payroll.</summary>
    BusinessDomain = 5,

    /// <summary>Professional certifications such as Azure Certified, AWS Certified, and GCP Certified.</summary>
    Certification = 6
}
