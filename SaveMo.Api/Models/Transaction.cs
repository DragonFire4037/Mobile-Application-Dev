
namespace SaveMo.Api.Models;

/// <summary>
/// Internal representation of a stored financial transaction.
/// </summary>
public sealed class Transaction
{
    public int Id { get; set; }

    public required string Description { get; set; }

    public decimal Amount { get; set; }

    public TransactionType Type { get; set; }

    public required string Category { get; set; }

    public DateOnly TransactionDate { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
