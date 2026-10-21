using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using SaveMo.Models;
using SaveMo.Services.Interfaces;

namespace SaveMo.ViewModels;

public class ReportsViewModel : INotifyPropertyChanged
{
    private readonly IReportService _reportService;
    private readonly IBudgetRepository _budgetRepository;

    private Report? _monthlyReport;
    private Report? _spendingTrends;
    private ObservableCollection<CategorySpending> _budgetVsActual = null!;
    private ObservableCollection<CategorySpending> _categoryBreakdown = null!;
    private int _selectedMonth;
    private int _selectedYear;
    private bool _isLoading;

    public Report? MonthlyReport
    {
        get => _monthlyReport;
        set => SetProperty(ref _monthlyReport, value);
    }

    public Report? SpendingTrends
    {
        get => _spendingTrends;
        set => SetProperty(ref _spendingTrends, value);
    }

    public ObservableCollection<CategorySpending> BudgetVsActual
    {
        get => _budgetVsActual;
        set => SetProperty(ref _budgetVsActual, value);
    }

    public ObservableCollection<CategorySpending> CategoryBreakdown
    {
        get => _categoryBreakdown;
        set => SetProperty(ref _categoryBreakdown, value);
    }

    public int SelectedMonth
    {
        get => _selectedMonth;
        set => SetProperty(ref _selectedMonth, value);
    }

    public int SelectedYear
    {
        get => _selectedYear;
        set => SetProperty(ref _selectedYear, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public ICommand LoadCommand { get; }
    public ICommand PreviousMonthCommand { get; }
    public ICommand NextMonthCommand { get; }

    public ReportsViewModel(IReportService reportService, IBudgetRepository budgetRepository)
    {
        _reportService = reportService;
        _budgetRepository = budgetRepository;

        var today = DateTime.Now;
        SelectedMonth = today.Month;
        SelectedYear = today.Year;

        BudgetVsActual = new ObservableCollection<CategorySpending>();
        CategoryBreakdown = new ObservableCollection<CategorySpending>();

        LoadCommand = new Command(async () => await LoadAsync());
        PreviousMonthCommand = new Command(() => GoToPreviousMonth());
        NextMonthCommand = new Command(() => GoToNextMonth());
    }

    public async Task LoadAsync()
    {
        try
        {
            IsLoading = true;

            // Parallelize all report loading tasks to avoid sequential waiting
            var monthlyReportTask = _reportService.GetMonthlyReportAsync(SelectedMonth, SelectedYear);
            var spendingTrendsTask = _reportService.GetSpendingTrendsAsync(6);
            var budgetVsActualTask = _reportService.GetBudgetVsActualAsync(SelectedMonth, SelectedYear);

            // Wait for all tasks to complete in parallel
            await Task.WhenAll(monthlyReportTask, spendingTrendsTask, budgetVsActualTask);

            // Get results after all complete
            MonthlyReport = await monthlyReportTask;
            SpendingTrends = await spendingTrendsTask;
            var budgetVsActualData = (await budgetVsActualTask).ToList();

            // Batch update collections
            BudgetVsActual.Clear();
            foreach (var item in budgetVsActualData)
            {
                BudgetVsActual.Add(item);
            }

            // Populate category breakdown from monthly report
            CategoryBreakdown.Clear();
            foreach (var category in MonthlyReport.CategoryBreakdown)
            {
                CategoryBreakdown.Add(category);
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Failed to load reports: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void GoToPreviousMonth()
    {
        var date = new DateTime(SelectedYear, SelectedMonth, 1);
        date = date.AddMonths(-1);
        SelectedMonth = date.Month;
        SelectedYear = date.Year;
        MainThread.BeginInvokeOnMainThread(async () => await LoadAsync());
    }

    private void GoToNextMonth()
    {
        var date = new DateTime(SelectedYear, SelectedMonth, 1);
        date = date.AddMonths(1);
        SelectedMonth = date.Month;
        SelectedYear = date.Year;
        MainThread.BeginInvokeOnMainThread(async () => await LoadAsync());
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
