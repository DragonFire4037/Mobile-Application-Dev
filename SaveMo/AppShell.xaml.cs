using SaveMo.Views;

namespace SaveMo
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Register routes for shell navigation
            Routing.RegisterRoute(nameof(DashboardPage), typeof(DashboardPage));
            Routing.RegisterRoute(nameof(TransactionsPage), typeof(TransactionsPage));
            Routing.RegisterRoute(nameof(BudgetsPage), typeof(BudgetsPage));
            Routing.RegisterRoute(nameof(ReportsPage), typeof(ReportsPage));
            Routing.RegisterRoute(nameof(PrePurchaseAnalyzerPage), typeof(PrePurchaseAnalyzerPage));

            // Initialize theme button text
            UpdateThemeButtonText();
        }

        /// <summary>
        /// Handle theme toggle button click
        /// </summary>
        private void OnThemeToggleClicked(object sender, EventArgs e)
        {
            App.ToggleTheme();
            UpdateThemeButtonText();
            CloseFlyout();
        }

        /// <summary>
        /// Update the theme button text based on current theme
        /// </summary>
        private void UpdateThemeButtonText()
        {
            ThemeToggleButton.Text = App.IsDarkModeEnabled ? "☀️ Light Mode" : "🌙 Dark Mode";
        }

        /// <summary>
        /// Close the flyout menu after selection
        /// </summary>
        private void CloseFlyout()
        {
            FlyoutIsPresented = false;
        }
    }
}
