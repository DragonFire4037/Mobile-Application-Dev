using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using SaveMo.Models;
using SaveMo.Services.Interfaces;

namespace SaveMo.ViewModels;

public class BudgetsViewModel : INotifyPropertyChanged
{
    private readonly IBudgetRepository _budgetRepository;
    private readonly ITransactionRepository _transactionRepository;

    private ObservableCollection<Budget> _budgets = null!;
    private Budget? _selectedBudget;
    private string _newCategory = string.Empty;
    private decimal _newMonthlyLimit;
    private bool _isAddingMode;
    private IEnumerable<string> _categories = null!;

    public ObservableCollection<Budget> Budgets
    {
        get => _budgets;
        set => SetProperty(ref _budgets, value);
    }

    public Budget? SelectedBudget
    {
        get => _selectedBudget;
        set => SetProperty(ref _selectedBudget, value);
    }

    public string NewCategory
    {
        get => _newCategory;
        set => SetProperty(ref _newCategory, value ?? string.Empty);
    }

    public decimal NewMonthlyLimit
    {
        get => _newMonthlyLimit;
        set => SetProperty(ref _newMonthlyLimit, value);
    }

    public bool IsAddingMode
    {
        get => _isAddingMode;
        set => SetProperty(ref _isAddingMode, value);
    }

    public IEnumerable<string> Categories
    {
        get => _categories;
        set => SetProperty(ref _categories, value);
    }

    public ICommand LoadCommand { get; }
    public ICommand AddBudgetCommand { get; }
    public ICommand UpdateBudgetCommand { get; }
    public ICommand DeleteBudgetCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand StartAddNewCommand { get; }

    public BudgetsViewModel(IBudgetRepository budgetRepository, ITransactionRepository transactionRepository)
    {
        _budgetRepository = budgetRepository;
        _transactionRepository = transactionRepository;

        Budgets = new ObservableCollection<Budget>();

        LoadCommand = new Command(async () => await LoadAsync());
        AddBudgetCommand = new Command(async () => await AddBudgetAsync());
        UpdateBudgetCommand = new Command(async () => await UpdateBudgetAsync());
        DeleteBudgetCommand = new Command(async () => await DeleteBudgetAsync());
        CancelCommand = new Command(() => CancelEdit());
        StartAddNewCommand = new Command(() => StartAddNew());
    }

    public async Task LoadAsync()
    {
        try
        {
            var today = DateTime.Now;
            var budgets = (await _budgetRepository.GetByMonthAsync(today.Month, today.Year)).ToList();

            // Update current spend from transactions
            var thisMonth = new DateTime(today.Year, today.Month, 1);
            var endOfMonth = thisMonth.AddMonths(1).AddDays(-1);
            var transactions = (await _transactionRepository.GetByDateRangeAsync(thisMonth, endOfMonth)).ToList();

            // Move expensive calculations to background thread - convert O(n²) to O(n)
            var enrichedBudgets = await Task.Run(() =>
            {
                // Pre-calculate spending by category (single pass through transactions)
                var expensesByCategory = transactions
                    .Where(t => t.Type == TransactionType.Expense)
                    .GroupBy(t => t.Category)
                    .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));

                // Single pass through budgets with dictionary lookup (O(1) per budget)
                var sorted = budgets
                    .OrderBy(b => b.Category)
                    .ToList();

                // Update spending amounts
                foreach (var budget in sorted)
                {
                    budget.CurrentSpend = expensesByCategory
                        .TryGetValue(budget.Category, out var amount) ? amount : 0;
                }

                return sorted;
            });

            // Load available categories in parallel
            var categories = await _budgetRepository.GetCategoriesAsync();

            // Batch update UI
            Budgets.Clear();
            foreach (var budget in enrichedBudgets)
            {
                Budgets.Add(budget);
            }

            Categories = categories.ToList();
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Failed to load budgets: {ex.Message}", "OK");
        }
    }

    public async Task AddBudgetAsync()
    {
        if (NewMonthlyLimit <= 0 || string.IsNullOrWhiteSpace(NewCategory))
        {
            await Application.Current.MainPage.DisplayAlert("Validation Error", "Please enter category and limit.", "OK");
            return;
        }

        try
        {
            var today = DateTime.Now;
            var budget = new Budget
            {
                Category = NewCategory,
                MonthlyLimit = NewMonthlyLimit,
                CurrentSpend = 0,
                Month = today.Month,
                Year = today.Year
            };

            var created = await _budgetRepository.CreateAsync(budget);
            Budgets.Add(created);
            ResetForm();
            IsAddingMode = false;

            await Application.Current.MainPage.DisplayAlert("Success", "Budget created.", "OK");
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Failed to create budget: {ex.Message}", "OK");
        }
    }

    public async Task UpdateBudgetAsync()
    {
        if (SelectedBudget == null)
            return;

        try
        {
            SelectedBudget.MonthlyLimit = NewMonthlyLimit;
            await _budgetRepository.UpdateAsync(SelectedBudget);
            ResetForm();
            IsAddingMode = false;

            await Application.Current.MainPage.DisplayAlert("Success", "Budget updated.", "OK");
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Failed to update budget: {ex.Message}", "OK");
        }
    }

    public async Task DeleteBudgetAsync()
    {
        if (SelectedBudget == null)
            return;

        var confirm = await Application.Current.MainPage.DisplayAlert("Confirm", "Delete this budget?", "Yes", "No");
        if (!confirm)
            return;

        try
        {
            await _budgetRepository.DeleteAsync(SelectedBudget.Id);
            Budgets.Remove(SelectedBudget);
            ResetForm();
            IsAddingMode = false;

            await Application.Current.MainPage.DisplayAlert("Success", "Budget deleted.", "OK");
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Failed to delete budget: {ex.Message}", "OK");
        }
    }

    public void EditBudget(Budget budget)
    {
        SelectedBudget = budget;
        NewCategory = budget.Category;
        NewMonthlyLimit = budget.MonthlyLimit;
        IsAddingMode = true;
    }

    public void StartAddNew()
    {
        ResetForm();
        IsAddingMode = true;
    }

    private void CancelEdit()
    {
        ResetForm();
        IsAddingMode = false;
    }

    private void ResetForm()
    {
        SelectedBudget = null;
        NewCategory = string.Empty;
        NewMonthlyLimit = 0;
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
