namespace Prospera.Contracts.DTOs.Asset;

/// <summary>
/// Request to add a new asset
/// </summary>
public class AddAssetRequest
{
    /// <summary>
    /// Asset name/description
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Current value of the asset
    /// </summary>
    public decimal CurrentValue { get; set; }

    /// <summary>
    /// Type of asset as a string
    /// Valid values: Cash, Stock, Bond, RealEstate, Crypto, Car, Other
    /// </summary>
    public string Type { get; set; } = string.Empty;
}
