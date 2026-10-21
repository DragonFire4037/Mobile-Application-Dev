namespace SaveMo.Events;

/// <summary>
/// Custom event arguments for transaction deletion
/// </summary>
public class TransactionDeletedEventArgs : EventArgs
{
    public int TransactionId { get; set; }
}

/// <summary>
/// Custom event arguments for transaction edit request
/// </summary>
public class TransactionEditRequestedEventArgs : EventArgs
{
    public int TransactionId { get; set; }
}

/// <summary>
/// Event handler delegate for transaction deleted events
/// </summary>
public delegate void TransactionDeletedEventHandler(object sender, TransactionDeletedEventArgs e);

/// <summary>
/// Event handler delegate for transaction edit requested events
/// </summary>
public delegate void TransactionEditRequestedEventHandler(object sender, TransactionEditRequestedEventArgs e);
