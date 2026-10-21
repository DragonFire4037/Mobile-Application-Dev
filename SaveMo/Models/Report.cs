using System.Collections.ObjectModel;

namespace SaveMo.Models;

/// <summary>
/// Aggregated financial data for reporting.
/// Used for monthly summaries, category breakdowns, and spending trends.
/// </summary>
public class Report
{
    public int Month { get; set; }
    public int Year { get; set; }

    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }

    public decimal NetAmount => TotalIncome - TotalExpense;

    /// <summary>
    /// Breakdown of spending by category.
    /// </summary>
    public ObservableCollection<CategorySpending> CategoryBreakdown { get; set; } = new();

    /// <summary>
    /// Monthly spending trend data for charting.
    /// </summary>
    public ObservableCollection<MonthlyTrend> SpendingTrends { get; set; } = new();
}

/// <summary>
/// Category-level spending breakdown for reports.
/// </summary>
public class CategorySpending
{
    public string Category { get; set; }
    public decimal Amount { get; set; }
    public double Percentage { get; set; }
}

/// <summary>
/// Monthly trend data for spending visualization.
/// </summary>
public class MonthlyTrend
{
    public string Month { get; set; }
    public decimal Income { get; set; }
    public decimal Expense { get; set; }
}

/// <summary>
/// Pre-purchase analysis result for the analyzer feature.
/// </summary>
public class PrePurchaseAnalysis
{
    public decimal ItemCost { get; set; }
    public int MonthsToAnalyze { get; set; }
    public decimal AnnualReturnRate { get; set; }

    /// <summary>
    /// Total cost over the analysis period.
    /// </summary>
    public decimal TotalCostOverPeriod => ItemCost * MonthsToAnalyze;

    /// <summary>
    /// Opportunity cost if the money was invested instead.
    /// </summary>
    public decimal OpportunityCost
    {
        get
        {
            var monthlyRate = AnnualReturnRate / 12 / 100;
            var months = MonthsToAnalyze;
            // Compound interest: P * (1 + r)^n - P
            return ItemCost * (decimal)Math.Pow((double)(1 + monthlyRate), months) - ItemCost;
        }
    }

    /// <summary>
    /// Total cost including opportunity cost.
    /// </summary>
    public decimal TotalCostWithOpportunityCost => TotalCostOverPeriod + OpportunityCost;

    /// <summary>
    /// Summary message for the user.
    /// </summary>
    public string Summary
    {
        get
        {
            return $"Over {MonthsToAnalyze} months, this ${ItemCost:F2} purchase will cost you " +
                   $"${TotalCostWithOpportunityCost:F2} (including opportunity cost).";
        }
    }
}
