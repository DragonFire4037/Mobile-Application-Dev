using SaveMo.Models;
using SaveMo.Services;
using SaveMo.ViewModels;

namespace SaveMo.Views;

public partial class TransactionsPage : BasePage
{
    private TransactionEventPublisher _eventPublisher;

    public TransactionsPage(TransactionsViewModel viewModel, TransactionEventPublisher eventPublisher)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _eventPublisher = eventPublisher;

        // DO NOT subscribe to events here!
        // Subscriptions happen in OnAppearing instead
    }

    /// <summary>
    /// Load cached data immediately (synchronous)
    /// </summary>
    protected override void LoadCachedData()
    {
        // Cached data is already in ViewModel, just display it
        // No action needed - binding shows current state
    }

    /// <summary>
    /// Load fresh data asynchronously (doesn't block animation)
    /// </summary>
    protected override async Task RefreshDataAsync(CancellationToken cancellationToken)
    {
        if (BindingContext is TransactionsViewModel viewModel)
        {
            await viewModel.LoadAsync();
        }
    }

    /// <summary>
    /// Subscribe to events when page appears
    /// </summary>
    protected override async void OnAppearing()
    {
        // Subscribe to events BEFORE loading data
        // This ensures events are subscribed from the start
        _eventPublisher.TransactionDeleted += OnTransactionDeleted;
        _eventPublisher.TransactionEditRequested += OnTransactionEditRequested;

        // Then call base to load data
        base.OnAppearing();
    }

    /// <summary>
    /// Unsubscribe from events when page disappears
    /// </summary>
    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        // Unsubscribe from events to prevent memory leaks
        _eventPublisher.TransactionDeleted -= OnTransactionDeleted;
        _eventPublisher.TransactionEditRequested -= OnTransactionEditRequested;
    }

    /// <summary>
    /// Handle transaction deleted event
    /// Already on UI thread, no marshaling needed
    /// </summary>
    private void OnTransactionDeleted(object sender, Events.TransactionDeletedEventArgs e)
    {
        if (BindingContext is TransactionsViewModel viewModel)
        {
            var transaction = viewModel.Transactions.FirstOrDefault(t => t.Id == e.TransactionId);
            if (transaction != null)
            {
                viewModel.Transactions.Remove(transaction);
            }
        }
    }

    /// <summary>
    /// Handle transaction edit requested event
    /// Already on UI thread, no marshaling needed
    /// </summary>
    private void OnTransactionEditRequested(object sender, Events.TransactionEditRequestedEventArgs e)
    {
        if (BindingContext is TransactionsViewModel viewModel)
        {
            var transaction = viewModel.Transactions.FirstOrDefault(t => t.Id == e.TransactionId);
            if (transaction != null)
            {
                viewModel.EditTransaction(transaction);
            }
        }
    }

    private void OnAddNewClicked(object sender, EventArgs e)
    {
        if (BindingContext is TransactionsViewModel viewModel)
        {
            viewModel.StartAddNew();
        }
    }

    private void OnEditClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.BindingContext is Transaction transaction)
        {
            if (BindingContext is TransactionsViewModel viewModel)
            {
                viewModel.EditTransaction(transaction);
            }
        }
    }

    /// <summary>
    /// Handle SwipeView left-swipe delete action
    /// </summary>
    private async void OnSwipeDelete(object sender, EventArgs e)
    {
        if (sender is SwipeItem swipeItem && swipeItem.CommandParameter is int transactionId)
        {
            if (BindingContext is TransactionsViewModel viewModel)
            {
                await viewModel.DeleteTransactionByIdAsync(transactionId);
            }
        }
    }

    /// <summary>
    /// Handle card tap for edit request
    /// </summary>
    private void OnCardLongPressed(object sender, TappedEventArgs e)
    {
        if (e.Parameter is Transaction transaction)
        {
            if (BindingContext is TransactionsViewModel viewModel)
            {
                viewModel.RaiseTransactionEditRequested(transaction.Id);
                viewModel.EditTransaction(transaction);
            }
        }
    }
}
