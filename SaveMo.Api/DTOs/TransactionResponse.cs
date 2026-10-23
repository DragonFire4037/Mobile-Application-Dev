namespace SaveMo.Api.DTOs;

/// <summary>
/// Public representation of a transaction returned by the API.
/// </summary>
public sealed class TransactionResponse
{
    public int Id { get; init; }

    public required string Description { get; init; }

    public decimal Amount { get; init; }

    public required string Type { get; init; }

    public required string Category { get; init; }

    public DateOnly TransactionDate { get; init; }

    public string? Notes { get; init; }

    public DateTime CreatedAtUtc { get; init; }
}
