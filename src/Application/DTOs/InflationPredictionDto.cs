namespace Prospera.Application.DTOs;

/// <summary>
/// DTO for inflation predictions
/// </summary>
public class InflationPredictionDto
{
    /// <summary>
    /// Country code for the prediction
    /// </summary>
    public string CountryCode { get; set; } = string.Empty;

    /// <summary>
    /// Current inflation rate (%)
    /// </summary>
    public decimal CurrentInflationRate { get; set; }

    /// <summary>
    /// Method used for predictions
    /// </summary>
    public string PredictionMethod { get; set; } = string.Empty;

    /// <summary>
    /// Yearly inflation predictions
    /// </summary>
    public List<InflationYearPredictionDto> YearlyPredictions { get; set; } = new();

    /// <summary>
    /// Additional notes about the predictions
    /// </summary>
    public string Notes { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when predictions were generated
    /// </summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Inflation prediction for a specific year
/// </summary>
public class InflationYearPredictionDto
{
    /// <summary>
    /// Prediction year
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// Predicted inflation rate (%)
    /// </summary>
    public decimal PredictedInflationRate { get; set; }

    /// <summary>
    /// Confidence level of the prediction (0-1)
    /// </summary>
    public decimal Confidence { get; set; }
}
