namespace GeldGuide.Core.Models;

/// <summary>
/// Employment status of the user. German labels per the GeldGuide spec.
/// </summary>
public enum EmploymentType
{
    /// <summary>Employee (Angestellt).</summary>
    Angestellt,

    /// <summary>Self-employed (Selbständig).</summary>
    Selbstaendig,

    /// <summary>Civil servant (Beamter).</summary>
    Beamter
}

/// <summary>
/// Marital status of the user.
/// </summary>
public enum MaritalStatus
{
    Single,
    Married,
    Divorced
}
