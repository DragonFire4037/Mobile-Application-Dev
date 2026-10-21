using SaveMo.Models;
using SaveMo.ViewModels;

namespace SaveMo.Views;

public partial class BudgetsPage : BasePage
{
    public BudgetsPage(BudgetsViewModel viewModel)
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
        if (BindingContext is BudgetsViewModel viewModel)
        {
            await viewModel.LoadAsync();
        }
    }

    private void OnAddNewClicked(object sender, EventArgs e)
    {
        if (BindingContext is BudgetsViewModel viewModel)
        {
            viewModel.StartAddNew();
        }
    }

    private void OnEditClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.BindingContext is Budget budget)
        {
            if (BindingContext is BudgetsViewModel viewModel)
            {
                viewModel.EditBudget(budget);
            }
        }
    }
}
