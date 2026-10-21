using SaveMo.ViewModels;

namespace SaveMo.Views;

public partial class DashboardPage : BasePage
{
    public DashboardPage(DashboardViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    /// <summary>
    /// Load cached data immediately (synchronous, doesn't block animation)
    /// </summary>
    protected override void LoadCachedData()
    {
        // Cached data is already in the ViewModel's properties
        // Just display whatever is there (old data from previous visit)
        // No action needed - binding already shows current ViewModel state
    }

    /// <summary>
    /// Refresh with fresh data asynchronously (doesn't block animation)
    /// </summary>
    protected override async Task RefreshDataAsync(CancellationToken cancellationToken)
    {
        if (BindingContext is DashboardViewModel viewModel)
        {
            await viewModel.LoadAsync();
        }
    }
}
