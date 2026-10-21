using SaveMo.Models;

namespace SaveMo.Services.Interfaces;

/// <summary>
/// Service interface for transaction CRUD operations.
/// Allows implementation swap (in-memory, API, database) without changing dependents.
/// </summary>
public interface ITransactionRepository
{
    /// <summary>
    /// Get all transactions.
    /// </summary>
    Task<IEnumerable<Transaction>> GetAllAsync();

    /// <summary>
    /// Get transaction by ID.
    /// </summary>
    Task<Transaction> GetByIdAsync(int id);

    /// <summary>
    /// Get transactions filtered by type and optional category.
    /// </summary>
    Task<IEnumerable<Transaction>> GetByFilterAsync(TransactionType? type = null, string category = null);

    /// <summary>
    /// Get transactions for a specific date range.
    /// </summary>
    Task<IEnumerable<Transaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Create a new transaction.
    /// </summary>
    Task<Transaction> CreateAsync(Transaction transaction);

    /// <summary>
    /// Update an existing transaction.
    /// </summary>
    Task<Transaction> UpdateAsync(Transaction transaction);

    /// <summary>
    /// Delete a transaction by ID.
    /// </summary>
    Task<bool> DeleteAsync(int id);
}
