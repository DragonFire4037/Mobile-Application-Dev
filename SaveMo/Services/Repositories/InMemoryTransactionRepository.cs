using SaveMo.Models;
using SaveMo.Services.Interfaces;

namespace SaveMo.Services.Repositories;

public class InMemoryTransactionRepository : ITransactionRepository
{
    private readonly List<Transaction> _transactions;
    private int _nextId;

    public InMemoryTransactionRepository()
    {
        _transactions = new List<Transaction>();
        _nextId = 1;
        _initializeSampleData();
    }

    public Task<IEnumerable<Transaction>> GetAllAsync()
    {
        return Task.FromResult(_transactions.AsEnumerable());
    }

    public Task<Transaction> GetByIdAsync(int id)
    {
        var transaction = _transactions.FirstOrDefault(t => t.Id == id);
        return Task.FromResult(transaction);
    }

    public Task<IEnumerable<Transaction>> GetByFilterAsync(TransactionType? type = null, string category = null)
    {
        var query = _transactions.AsEnumerable();

        if (type.HasValue)
            query = query.Where(t => t.Type == type.Value);

        if (!string.IsNullOrEmpty(category))
            query = query.Where(t => t.Category == category);

        return Task.FromResult(query);
    }

    public Task<IEnumerable<Transaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var query = _transactions.Where(t => t.Date >= startDate && t.Date <= endDate);
        return Task.FromResult(query.AsEnumerable());
    }

    public Task<Transaction> CreateAsync(Transaction transaction)
    {
        transaction.Id = _nextId++;
        _transactions.Add(transaction);
        return Task.FromResult(transaction);
    }

    public Task<Transaction> UpdateAsync(Transaction transaction)
    {
        var existing = _transactions.FirstOrDefault(t => t.Id == transaction.Id);
        if (existing == null)
            return Task.FromResult<Transaction>(null);

        existing.Amount = transaction.Amount;
        existing.Type = transaction.Type;
        existing.Category = transaction.Category;
        existing.Date = transaction.Date;
        existing.Note = transaction.Note;

        return Task.FromResult(existing);
    }

    public Task<bool> DeleteAsync(int id)
    {
        var transaction = _transactions.FirstOrDefault(t => t.Id == id);
        if (transaction == null)
            return Task.FromResult(false);

        _transactions.Remove(transaction);
        return Task.FromResult(true);
    }

    private void _initializeSampleData()
    {
        var today = DateTime.Now;
        var thisMonth = new DateTime(today.Year, today.Month, 1);

        _transactions.Add(new Transaction
        {
            Id = _nextId++,
            Amount = 3000,
            Type = TransactionType.Income,
            Category = "Salary",
            Date = thisMonth,
            Note = "Monthly salary"
        });

        _transactions.Add(new Transaction
        {
            Id = _nextId++,
            Amount = 50,
            Type = TransactionType.Expense,
            Category = "Food",
            Date = today.AddDays(-5),
            Note = "Groceries"
        });

        _transactions.Add(new Transaction
        {
            Id = _nextId++,
            Amount = 150,
            Type = TransactionType.Expense,
            Category = "Utilities",
            Date = today.AddDays(-10),
            Note = "Internet bill"
        });

        _transactions.Add(new Transaction
        {
            Id = _nextId++,
            Amount = 200,
            Type = TransactionType.Expense,
            Category = "Entertainment",
            Date = today.AddDays(-3),
            Note = "Movie tickets and dinner"
        });

        _transactions.Add(new Transaction
        {
            Id = _nextId++,
            Amount = 25,
            Type = TransactionType.Expense,
            Category = "Transport",
            Date = today.AddDays(-2),
            Note = "Gas"
        });
    }
}
