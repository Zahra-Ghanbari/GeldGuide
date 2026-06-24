namespace GeldGuide.Core.Models;

/// <summary>
/// The single on-device record holding all user-supplied data used to evaluate
/// the 15 financial rules. Every field is nullable: fields are collected
/// progressively (3-tier), so an unset field means "not yet answered".
///
/// For the tri-state yes/no/don't-know fields, <c>null</c> means "don't know".
/// </summary>
public class UserProfile
{
    // --- Demographics ---

    /// <summary>User age in years. (Tier 2)</summary>
    public int? Age { get; set; }

    /// <summary>Employment status. (Tier 2)</summary>
    public EmploymentType? EmploymentType { get; set; }

    /// <summary>Marital status. (Tier 2)</summary>
    public MaritalStatus? MaritalStatus { get; set; }

    // --- Income & Expenses ---

    /// <summary>Net monthly salary in euros. (Tier 1)</summary>
    public decimal? NetMonthlySalary { get; set; }

    /// <summary>Gross annual salary in euros. (Tier 2)</summary>
    public decimal? GrossAnnualSalary { get; set; }

    /// <summary>Monthly fixed expenses in euros. (Tier 1)</summary>
    public decimal? MonthlyFixedExpenses { get; set; }

    /// <summary>Monthly variable expenses in euros. (Tier 1)</summary>
    public decimal? MonthlyVariableExpenses { get; set; }

    // --- Savings & Debt ---

    /// <summary>Current total savings in euros. (Tier 1)</summary>
    public decimal? CurrentSavings { get; set; }

    /// <summary>Amount saved per month in euros. (Tier 2)</summary>
    public decimal? MonthlySavingsAmount { get; set; }

    /// <summary>Whether the user has high-interest debt (Dispo, credit cards). (Tier 1)</summary>
    public bool? HasHighInterestDebt { get; set; }

    /// <summary>Detailed list of debts. (Tier 2)</summary>
    public List<Debt>? Debts { get; set; }

    /// <summary>List of recurring subscriptions. (Tier 2)</summary>
    public List<Subscription>? Subscriptions { get; set; }

    // --- Protection ---

    /// <summary>Has private liability insurance (Privathaftpflicht). null = don't know. (Tier 2)</summary>
    public bool? HasPrivathaftpflicht { get; set; }

    /// <summary>Has occupational disability insurance (BU). null = don't know. (Tier 2)</summary>
    public bool? HasBU { get; set; }

    // --- Family & Subsidies ---

    /// <summary>Number of children. (Tier 1)</summary>
    public int? ChildrenCount { get; set; }

    /// <summary>Ages of the children in years. (Tier 2)</summary>
    public int[]? ChildrenAges { get; set; }

    /// <summary>Currently receiving Kindergeld. null = don't know. (Tier 2)</summary>
    public bool? ReceivingKindergeld { get; set; }

    /// <summary>Currently receiving Bürgergeld. (Tier 2)</summary>
    public bool? ReceivingBuergergeld { get; set; }

    /// <summary>Monthly childcare cost in euros. (Tier 2)</summary>
    public decimal? MonthlyChildcareCost { get; set; }

    /// <summary>Monthly rent cost in euros. (Tier 2)</summary>
    public decimal? MonthlyRentCost { get; set; }

    /// <summary>Monthly heating cost in euros. (Tier 2)</summary>
    public decimal? MonthlyHeatingCost { get; set; }

    /// <summary>Already receiving Kinderzuschlag. (Tier 2)</summary>
    public bool? AlreadyReceivingKinderzuschlag { get; set; }

    // --- Investing ---

    /// <summary>Employer offers VL contributions. null = don't know. (Tier 2)</summary>
    public bool? EmployerOffersVL { get; set; }

    // --- Tax ---

    /// <summary>One-way commute distance in km. (Tier 2)</summary>
    public decimal? CommuteDistanceKm { get; set; }

    /// <summary>Home-office days per week. (Tier 2)</summary>
    public int? HomeOfficeDaysPerWeek { get; set; }

    /// <summary>Claims the Pendlerpauschale. null = don't know. (Tier 2)</summary>
    public bool? ClaimsPendlerpauschale { get; set; }

    /// <summary>Freistellungsauftrag is set. null = don't know. (Tier 2)</summary>
    public bool? FreistellungsauftragSet { get; set; }

    // --- Retirement ---

    /// <summary>Expected state pension in euros per month. (Tier 2)</summary>
    public decimal? ExpectedStatePension { get; set; }

    /// <summary>Desired retirement income in euros per month. (Tier 2)</summary>
    public decimal? DesiredRetirementIncome { get; set; }
}
