using SaveMo.Models;
using SaveMo.Services.Interfaces;

namespace SaveMo.Services.Repositories;

public class InMemoryBudgetRepository : IBudgetRepository
{
    private readonly List<Budget> _budgets;
    private int _nextId;

    public InMemoryBudgetRepository()
    {
        _budgets = new List<Budget>();
        _nextId = 1;
        _initializeSampleData();
    }

    public Task<IEnumerable<Budget>> GetAllAsync()
    {
        return Task.FromResult(_budgets.AsEnumerable());
    }

    public Task<Budget> GetByIdAsync(int id)
    {
        var budget = _budgets.FirstOrDefault(b => b.Id == id);
        return Task.FromResult(budget);
    }

    public Task<IEnumerable<Budget>> GetByMonthAsync(int month, int year)
    {
        var query = _budgets.Where(b => b.Month == month && b.Year == year);
        return Task.FromResult(query.AsEnumerable());
    }

    public Task<Budget> GetByMonthAndCategoryAsync(int month, int year, string category)
    {
        var budget = _budgets.FirstOrDefault(b =>
            b.Month == month && b.Year == year && b.Category == category);
        return Task.FromResult(budget);
    }

    public Task<Budget> CreateAsync(Budget budget)
    {
        budget.Id = _nextId++;
        _budgets.Add(budget);
        return Task.FromResult(budget);
    }

    public Task<Budget> UpdateAsync(Budget budget)
    {
        var existing = _budgets.FirstOrDefault(b => b.Id == budget.Id);
        if (existing == null)
            return Task.FromResult<Budget>(null);

        existing.Category = budget.Category;
        existing.MonthlyLimit = budget.MonthlyLimit;
        existing.CurrentSpend = budget.CurrentSpend;
        existing.Month = budget.Month;
        existing.Year = budget.Year;

        return Task.FromResult(existing);
    }

    public Task<bool> DeleteAsync(int id)
    {
        var budget = _budgets.FirstOrDefault(b => b.Id == id);
        if (budget == null)
            return Task.FromResult(false);

        _budgets.Remove(budget);
        return Task.FromResult(true);
    }

    public Task<IEnumerable<string>> GetCategoriesAsync()
    {
        var categories = _budgets.Select(b => b.Category).Distinct().OrderBy(c => c).AsEnumerable();
        return Task.FromResult(categories);
    }

    private void _initializeSampleData()
    {
        var today = DateTime.Now;
        var month = today.Month;
        var year = today.Year;

        _budgets.Add(new Budget
        {
            Id = _nextId++,
            Category = "Food",
            MonthlyLimit = 400,
            CurrentSpend = 125,
            Month = month,
            Year = year
        });

        _budgets.Add(new Budget
        {
            Id = _nextId++,
            Category = "Utilities",
            MonthlyLimit = 200,
            CurrentSpend = 150,
            Month = month,
            Year = year
        });

        _budgets.Add(new Budget
        {
            Id = _nextId++,
            Category = "Entertainment",
            MonthlyLimit = 150,
            CurrentSpend = 200, // Over budget
            Month = month,
            Year = year
        });

        _budgets.Add(new Budget
        {
            Id = _nextId++,
            Category = "Transport",
            MonthlyLimit = 100,
            CurrentSpend = 25,
            Month = month,
            Year = year
        });
    }
}
