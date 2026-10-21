using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using SaveMo.Models;
using SaveMo.Services.Interfaces;

namespace SaveMo.ViewModels;

public class DashboardViewModel : INotifyPropertyChanged
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IBudgetRepository _budgetRepository;
    private readonly IReportService _reportService;

    private decimal _totalIncome;
    private decimal _totalExpense;
    private decimal _currentBalance;
    private ObservableCollection<Budget> _budgetsOverview;
    private ObservableCollection<Transaction> _recentTransactions;

    public decimal TotalIncome
    {
        get => _totalIncome;
        set => SetProperty(ref _totalIncome, value);
    }

    public decimal TotalExpense
    {
        get => _totalExpense;
        set => SetProperty(ref _totalExpense, value);
    }

    public decimal CurrentBalance
    {
        get => _currentBalance;
        set => SetProperty(ref _currentBalance, value);
    }

    public ObservableCollection<Budget> BudgetsOverview
    {
        get => _budgetsOverview;
        set => SetProperty(ref _budgetsOverview, value);
    }

    public ObservableCollection<Transaction> RecentTransactions
    {
        get => _recentTransactions;
        set => SetProperty(ref _recentTransactions, value);
    }

    public ICommand LoadCommand { get; }
    public ICommand NavigateToTransactionsCommand { get; }
    public ICommand NavigateToBudgetsCommand { get; }
    public ICommand NavigateToReportsCommand { get; }

    public DashboardViewModel(
        ITransactionRepository transactionRepository,
        IBudgetRepository budgetRepository,
        IReportService reportService)
    {
        _transactionRepository = transactionRepository;
        _budgetRepository = budgetRepository;
        _reportService = reportService;

        BudgetsOverview = new ObservableCollection<Budget>();
        RecentTransactions = new ObservableCollection<Transaction>();

        LoadCommand = new Command(async () => await LoadAsync());
        NavigateToTransactionsCommand = new Command(async () => await NavigateToTransactions());
        NavigateToBudgetsCommand = new Command(async () => await NavigateToBudgets());
        NavigateToReportsCommand = new Command(async () => await NavigateToReports());
    }

    public async Task LoadAsync()
    {
        try
        {
            var today = DateTime.Now;
            var thisMonth = new DateTime(today.Year, today.Month, 1);
            var endOfMonth = thisMonth.AddMonths(1).AddDays(-1);

            // Get current month transactions
            var transactions = (await _transactionRepository.GetByDateRangeAsync(thisMonth, endOfMonth)).ToList();

            // Move LINQ calculations to background thread to avoid blocking UI
            var calculationTask = Task.Run(() =>
            {
                return new
                {
                    Income = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount),
                    Expense = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount),
                    RecentTransactions = transactions.OrderByDescending(t => t.Date).Take(5).ToList()
                };
            });

            // Get current month budgets (can happen in parallel)
            var budgets = (await _budgetRepository.GetByMonthAsync(today.Month, today.Year)).ToList();

            // Wait for LINQ calculations to complete
            var calculations = await calculationTask;

            // Batch update properties to minimize PropertyChanged events
            TotalIncome = calculations.Income;
            TotalExpense = calculations.Expense;
            CurrentBalance = calculations.Income - calculations.Expense;

            // Update collections in single batch
            BudgetsOverview.Clear();
            foreach (var budget in budgets.Take(4))
            {
                BudgetsOverview.Add(budget);
            }

            RecentTransactions.Clear();
            foreach (var transaction in calculations.RecentTransactions)
            {
                RecentTransactions.Add(transaction);
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Failed to load dashboard: {ex.Message}", "OK");
        }
    }

    private async Task NavigateToTransactions()
    {
        await Shell.Current.GoToAsync("transactions");
    }

    private async Task NavigateToBudgets()
    {
        await Shell.Current.GoToAsync("budgets");
    }

    private async Task NavigateToReports()
    {
        await Shell.Current.GoToAsync("reports");
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
    {
        if (Equals(storage, value))
            return false;

        storage = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
