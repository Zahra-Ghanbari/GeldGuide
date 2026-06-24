namespace GeldGuide.Core.Models;

/// <summary>
/// A single debt held by the user, used by the Debt Kill-Switch rule.
/// </summary>
public class Debt
{
    /// <summary>Kind of debt (e.g. Dispo, credit card, loan).</summary>
    public string? Type { get; set; }

    /// <summary>Outstanding balance in euros.</summary>
    public decimal? Balance { get; set; }

    /// <summary>Annual interest rate as a percentage (e.g. 12.5 for 12.5%).</summary>
    public decimal? InterestRate { get; set; }
}
