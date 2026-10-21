using SaveMo.ViewModels;

namespace SaveMo.Views;

public partial class PrePurchaseAnalyzerPage : BasePage
{
    public PrePurchaseAnalyzerPage(PrePurchaseAnalyzerViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
