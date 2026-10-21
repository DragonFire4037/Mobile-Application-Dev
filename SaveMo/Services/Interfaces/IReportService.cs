using SaveMo.Models;

namespace SaveMo.Services.Interfaces;

/// <summary>
/// Service interface for generating financial reports and analysis.
/// </summary>
public interface IReportService
{
    /// <summary>
    /// Generate a monthly report for the specified month and year.
    /// </summary>
    Task<Report> GetMonthlyReportAsync(int month, int year);

    /// <summary>
    /// Get spending trends over the past N months.
    /// </summary>
    Task<Report> GetSpendingTrendsAsync(int monthsBack = 6);

    /// <summary>
    /// Get budget vs. actual for the current month.
    /// </summary>
    Task<IEnumerable<CategorySpending>> GetBudgetVsActualAsync(int month, int year);

    /// <summary>
    /// Get all available categories across transactions and budgets.
    /// </summary>
    Task<IEnumerable<string>> GetCategoriesAsync();

    /// <summary>
    /// Analyze a potential purchase and return cost projections.
    /// </summary>
    Task<PrePurchaseAnalysis> AnalyzePurchaseAsync(decimal itemCost, int monthsToAnalyze, decimal annualReturnRate);
}
