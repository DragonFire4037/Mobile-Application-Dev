using SaveMo.Events;

namespace SaveMo.Services;

/// <summary>
/// Service to publish transaction-related events
/// </summary>
public class TransactionEventPublisher
{
    public event TransactionDeletedEventHandler TransactionDeleted;
    public event TransactionEditRequestedEventHandler TransactionEditRequested;

    public void RaiseTransactionDeleted(int transactionId)
    {
        TransactionDeleted?.Invoke(this, new TransactionDeletedEventArgs { TransactionId = transactionId });
    }

    public void RaiseTransactionEditRequested(int transactionId)
    {
        TransactionEditRequested?.Invoke(this, new TransactionEditRequestedEventArgs { TransactionId = transactionId });
    }
}
