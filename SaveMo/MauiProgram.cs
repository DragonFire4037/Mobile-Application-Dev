using Microsoft.Extensions.Logging;
using SaveMo.Services;
using SaveMo.Services.Interfaces;
using SaveMo.Services.Repositories;
using SaveMo.ViewModels;
using SaveMo.Views;

namespace SaveMo
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Register Services
            builder.Services.AddSingleton<ITransactionRepository, InMemoryTransactionRepository>();
            builder.Services.AddSingleton<IBudgetRepository, InMemoryBudgetRepository>();
            builder.Services.AddSingleton<IReportService, ReportService>();
            builder.Services.AddSingleton<TransactionEventPublisher>();

            // Register ViewModels as Singletons (shared state)
            builder.Services.AddSingleton<DashboardViewModel>();
            builder.Services.AddSingleton<TransactionsViewModel>();
            builder.Services.AddSingleton<BudgetsViewModel>();
            builder.Services.AddSingleton<ReportsViewModel>();
            builder.Services.AddSingleton<PrePurchaseAnalyzerViewModel>();

            // Register Pages as Transients (fresh instance each time)
            // This ensures smooth page transitions without animation blocking
            builder.Services.AddTransient<DashboardPage>();
            builder.Services.AddTransient<TransactionsPage>();
            builder.Services.AddTransient<BudgetsPage>();
            builder.Services.AddTransient<ReportsPage>();
            builder.Services.AddTransient<PrePurchaseAnalyzerPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
