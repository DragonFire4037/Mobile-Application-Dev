using SaveMo.Models;
using SaveMo.Services.Interfaces;

namespace SaveMo.Services.Repositories;

public class ReportService : IReportService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IBudgetRepository _budgetRepository;

    public ReportService(ITransactionRepository transactionRepository, IBudgetRepository budgetRepository)
    {
        _transactionRepository = transactionRepository;
        _budgetRepository = budgetRepository;
    }

    public async Task<Report> GetMonthlyReportAsync(int month, int year)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        var transactions = await _transactionRepository.GetByDateRangeAsync(startDate, endDate);

        var report = new Report
        {
            Month = month,
            Year = year,
            TotalIncome = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount),
            TotalExpense = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount)
        };

        // Build category breakdown
        var expensesByCategory = transactions
            .Where(t => t.Type == TransactionType.Expense)
            .GroupBy(t => t.Category)
            .Select(g => new { Category = g.Key, Amount = g.Sum(t => t.Amount) })
            .ToList();

        var totalExpense = expensesByCategory.Sum(x => x.Amount);

        foreach (var category in expensesByCategory)
        {
            report.CategoryBreakdown.Add(new CategorySpending
            {
                Category = category.Category,
                Amount = category.Amount,
                Percentage = totalExpense > 0 ? (double)(category.Amount / totalExpense) * 100 : 0
            });
        }

        return report;
    }

    public async Task<Report> GetSpendingTrendsAsync(int monthsBack = 6)
    {
        var today = DateTime.Now;

        // Create tasks for all months in parallel instead of sequential awaits
        var monthTasks = Enumerable.Range(0, monthsBack)
            .Select(i =>
            {
                var targetDate = today.AddMonths(-i);
                var month = targetDate.Month;
                var year = targetDate.Year;

                var startDate = new DateTime(year, month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                // Create a task that fetches and calculates for this month
                return _transactionRepository.GetByDateRangeAsync(startDate, endDate)
                    .ContinueWith(task =>
                    {
                        var transactions = task.Result;
                        return new MonthlyTrend
                        {
                            Month = targetDate.ToString("MMM yyyy"),
                            Income = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount),
                            Expense = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount)
                        };
                    });
            })
            .ToList();

        // Wait for all month tasks to complete in parallel
        var monthlyTrends = await Task.WhenAll(monthTasks);

        // Build report with completed trends
        var trends = new Report();
        trends.SpendingTrends.Clear();
        foreach (var trend in monthlyTrends.OrderBy(t => t.Month))
        {
            trends.SpendingTrends.Add(trend);
        }

        return trends;

    }

    public async Task<IEnumerable<CategorySpending>> GetBudgetVsActualAsync(int month, int year)
    {
        var budgets = (await _budgetRepository.GetByMonthAsync(month, year)).ToList();
        var result = new List<CategorySpending>();

        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);
        var transactions = (await _transactionRepository.GetByDateRangeAsync(startDate, endDate)).ToList();

        foreach (var budget in budgets)
        {
            var actual = transactions
                .Where(t => t.Type == TransactionType.Expense && t.Category == budget.Category)
                .Sum(t => t.Amount);

            result.Add(new CategorySpending
            {
                Category = budget.Category,
                Amount = actual,
                Percentage = budget.MonthlyLimit > 0 ? (double)(actual / budget.MonthlyLimit) * 100 : 0
            });
        }

        return result;
    }

    public async Task<IEnumerable<string>> GetCategoriesAsync()
    {
        var transactionCategories = (await _transactionRepository.GetByFilterAsync())
            .Select(t => t.Category)
            .Distinct();

        var budgetCategories = await _budgetRepository.GetCategoriesAsync();

        return transactionCategories.Union(budgetCategories).OrderBy(c => c);
    }

    public Task<PrePurchaseAnalysis> AnalyzePurchaseAsync(decimal itemCost, int monthsToAnalyze, decimal annualReturnRate)
    {
        var analysis = new PrePurchaseAnalysis
        {
            ItemCost = itemCost,
            MonthsToAnalyze = monthsToAnalyze,
            AnnualReturnRate = annualReturnRate
        };

        return Task.FromResult(analysis);
    }
}
