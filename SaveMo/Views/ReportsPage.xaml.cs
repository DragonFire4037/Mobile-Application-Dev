using SaveMo.ViewModels;

namespace SaveMo.Views;

public partial class ReportsPage : BasePage
{
    public ReportsPage(ReportsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    /// <summary>
    /// Load cached data immediately (synchronous)
    /// </summary>
    protected override void LoadCachedData()
    {
        // Cached data is already in ViewModel, display it immediately
        // No action needed - binding shows current state
    }

    /// <summary>
    /// Refresh with fresh data asynchronously (doesn't block animation)
    /// </summary>
    protected override async Task RefreshDataAsync(CancellationToken cancellationToken)
    {
        if (BindingContext is ReportsViewModel viewModel)
        {
            await viewModel.LoadAsync();
        }
    }
}
