namespace GeldGuide.Core.Models;

/// <summary>
/// A recurring subscription, used by the Ghost Audit (Abo-Check) rule.
/// </summary>
public class Subscription
{
    /// <summary>Name of the subscription (e.g. Netflix, gym).</summary>
    public string? Name { get; set; }

    /// <summary>Monthly cost in euros.</summary>
    public decimal? MonthlyCost { get; set; }
}
