using System.ComponentModel.DataAnnotations;

namespace SaveMo.Api.DTOs;

/// <summary>
/// Client-supplied fields for creating a transaction.
/// Server-controlled fields are deliberately excluded.
/// </summary>
public sealed class CreateTransactionRequest
{
    [Required(ErrorMessage = "Description is required.")]
    [StringLength(
        200,
        MinimumLength = 1,
        ErrorMessage = "Description must be between 1 and 200 characters.")]
    public string Description { get; init; } = string.Empty;

    [Range(
        typeof(decimal),
        "0.01",
        "999999999.99",
        ErrorMessage = "Amount must be greater than zero and no more than 999999999.99.")]
    public decimal Amount { get; init; }

    [Required(ErrorMessage = "Type is required.")]
    [RegularExpression(
        "^(Income|Expense|Transfer)$",
        ErrorMessage = "Type must be Income, Expense, or Transfer.")]
    public string Type { get; init; } = string.Empty;

    [Required(ErrorMessage = "Category is required.")]
    [StringLength(
        100,
        MinimumLength = 1,
        ErrorMessage = "Category must be between 1 and 100 characters.")]
    public string Category { get; init; } = string.Empty;

    [Required(ErrorMessage = "TransactionDate is required.")]
    public DateOnly? TransactionDate { get; init; }

    [StringLength(
        1000,
        ErrorMessage = "Notes cannot exceed 1000 characters.")]
    public string? Notes { get; init; }
}
