using SaveMo.Models;

namespace SaveMo.Services.Interfaces;

/// <summary>
/// Service interface for budget management.
/// Allows implementation swap (in-memory, API, database) without changing dependents.
/// </summary>
public interface IBudgetRepository
{
    /// <summary>
    /// Get all budgets.
    /// </summary>
    Task<IEnumerable<Budget>> GetAllAsync();

    /// <summary>
    /// Get budget by ID.
    /// </summary>
    Task<Budget> GetByIdAsync(int id);

    /// <summary>
    /// Get budgets for a specific month and year.
    /// </summary>
    Task<IEnumerable<Budget>> GetByMonthAsync(int month, int year);

    /// <summary>
    /// Get budget for a specific category and month.
    /// </summary>
    Task<Budget> GetByMonthAndCategoryAsync(int month, int year, string category);

    /// <summary>
    /// Create a new budget.
    /// </summary>
    Task<Budget> CreateAsync(Budget budget);

    /// <summary>
    /// Update an existing budget.
    /// </summary>
    Task<Budget> UpdateAsync(Budget budget);

    /// <summary>
    /// Delete a budget by ID.
    /// </summary>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Get all categories with budgets defined.
    /// </summary>
    Task<IEnumerable<string>> GetCategoriesAsync();
}
