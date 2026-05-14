namespace Prospera.Contracts.DTOs.Liability;

/// <summary>
/// Request to add a new liability
/// </summary>
public class AddLiabilityRequest
{
    /// <summary>
    /// Liability name/description
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Amount owed
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Type of liability as a string
    /// Valid values: CreditCard, Loan, Mortgage, PersonalDebt, Other
    /// </summary>
    public string Type { get; set; } = string.Empty;
}
