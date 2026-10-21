using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using SaveMo.Models;
using SaveMo.Services;
using SaveMo.Services.Interfaces;

namespace SaveMo.ViewModels;

public class TransactionsViewModel : INotifyPropertyChanged
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IReportService _reportService;
    private readonly TransactionEventPublisher _eventPublisher;

    private ObservableCollection<Transaction> _transactions = null!;
    private Transaction? _selectedTransaction;
    private decimal _newAmount;
    private string _newCategory = string.Empty;
    private string _newNote = string.Empty;
    private TransactionType _newType;
    private DateTime _newDate = DateTime.Now;
    private bool _isAddingMode;
    private IEnumerable<string> _categories = null!;

    public ObservableCollection<Transaction> Transactions
    {
        get => _transactions;
        set => SetProperty(ref _transactions, value);
    }

    public Transaction? SelectedTransaction
    {
        get => _selectedTransaction;
        set => SetProperty(ref _selectedTransaction, value);
    }

    public decimal NewAmount
    {
        get => _newAmount;
        set => SetProperty(ref _newAmount, value);
    }

    public string NewCategory
    {
        get => _newCategory;
        set => SetProperty(ref _newCategory, value ?? string.Empty);
    }

    public string NewNote
    {
        get => _newNote;
        set => SetProperty(ref _newNote, value ?? string.Empty);
    }

    public TransactionType NewType
    {
        get => _newType;
        set => SetProperty(ref _newType, value);
    }

    public DateTime NewDate
    {
        get => _newDate;
        set => SetProperty(ref _newDate, value);
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
    public ICommand AddTransactionCommand { get; }
    public ICommand UpdateTransactionCommand { get; }
    public ICommand DeleteTransactionCommand { get; }
    public ICommand DeleteSwipeCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand EditCommand { get; }

    public TransactionsViewModel(ITransactionRepository transactionRepository, IReportService reportService, TransactionEventPublisher eventPublisher)
    {
        _transactionRepository = transactionRepository;
        _reportService = reportService;
        _eventPublisher = eventPublisher;

        Transactions = new ObservableCollection<Transaction>();
        NewDate = DateTime.Now;
        NewType = TransactionType.Expense;

        LoadCommand = new Command(async () => await LoadAsync());
        AddTransactionCommand = new Command(async () => await AddTransactionAsync());
        UpdateTransactionCommand = new Command(async () => await UpdateTransactionAsync());
        DeleteTransactionCommand = new Command(async () => await DeleteTransactionAsync());
        DeleteSwipeCommand = new Command<int>(async (id) => await DeleteTransactionByIdAsync(id));
        CancelCommand = new Command(() => CancelEdit());
        EditCommand = new Command<Transaction>(transaction => EditTransaction(transaction));
    }

    public async Task LoadAsync()
    {
        try
        {
            // Fetch data from repositories
            var transactionsTask = _transactionRepository.GetAllAsync();
            var categoriesTask = _reportService.GetCategoriesAsync();

            // Wait for both data sources
            var transactions = await transactionsTask;
            var categories = await categoriesTask;

            // Move expensive sort operation to background thread
            var sortedTransactions = await Task.Run(() =>
            {
                return transactions
                    .OrderByDescending(t => t.Date)
                    .ToList();
            });

            // Batch update collections on UI thread
            Transactions.Clear();
            foreach (var transaction in sortedTransactions)
            {
                Transactions.Add(transaction);
            }

            Categories = categories.ToList();
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Failed to load transactions: {ex.Message}", "OK");
        }
    }

    public async Task AddTransactionAsync()
    {
        if (NewAmount <= 0 || string.IsNullOrWhiteSpace(NewCategory))
        {
            await Application.Current.MainPage.DisplayAlert("Validation Error", "Please fill in amount and category.", "OK");
            return;
        }

        try
        {
            var transaction = new Transaction
            {
                Amount = NewAmount,
                Type = NewType,
                Category = NewCategory,
                Date = NewDate,
                Note = NewNote
            };

            var created = await _transactionRepository.CreateAsync(transaction);
            Transactions.Insert(0, created);

            // Clear form
            ResetForm();
            IsAddingMode = false;

            await Application.Current.MainPage.DisplayAlert("Success", "Transaction added.", "OK");
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Failed to add transaction: {ex.Message}", "OK");
        }
    }

    public async Task UpdateTransactionAsync()
    {
        if (SelectedTransaction == null)
            return;

        try
        {
            SelectedTransaction.Amount = NewAmount;
            SelectedTransaction.Type = NewType;
            SelectedTransaction.Category = NewCategory;
            SelectedTransaction.Date = NewDate;
            SelectedTransaction.Note = NewNote;

            await _transactionRepository.UpdateAsync(SelectedTransaction);
            ResetForm();
            IsAddingMode = false;

            await Application.Current.MainPage.DisplayAlert("Success", "Transaction updated.", "OK");
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Failed to update transaction: {ex.Message}", "OK");
        }
    }

    public async Task DeleteTransactionAsync()
    {
        if (SelectedTransaction == null)
            return;

        var confirm = await Application.Current.MainPage.DisplayAlert("Confirm", "Delete this transaction?", "Yes", "No");
        if (!confirm)
            return;

        try
        {
            await _transactionRepository.DeleteAsync(SelectedTransaction.Id);
            Transactions.Remove(SelectedTransaction);
            ResetForm();
            IsAddingMode = false;

            await Application.Current.MainPage.DisplayAlert("Success", "Transaction deleted.", "OK");
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Failed to delete transaction: {ex.Message}", "OK");
        }
    }

    public async Task DeleteTransactionByIdAsync(int transactionId)
    {
        try
        {
            var transaction = Transactions.FirstOrDefault(t => t.Id == transactionId);
            if (transaction == null)
                return;

            var confirm = await Application.Current.MainPage.DisplayAlert("Confirm", "Delete this transaction?", "Yes", "No");
            if (!confirm)
                return;

            await _transactionRepository.DeleteAsync(transactionId);
            Transactions.Remove(transaction);

            // Raise the deleted event
            _eventPublisher.RaiseTransactionDeleted(transactionId);

            await Application.Current.MainPage.DisplayAlert("Success", "Transaction deleted.", "OK");
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Failed to delete transaction: {ex.Message}", "OK");
        }
    }

    public void RaiseTransactionEditRequested(int transactionId)
    {
        _eventPublisher.RaiseTransactionEditRequested(transactionId);
    }

    public void EditTransaction(Transaction transaction)
    {
        SelectedTransaction = transaction;
        NewAmount = transaction.Amount;
        NewType = transaction.Type;
        NewCategory = transaction.Category;
        NewDate = transaction.Date;
        NewNote = transaction.Note;
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
        SelectedTransaction = null;
        NewAmount = 0;
        NewCategory = string.Empty;
        NewNote = string.Empty;
        NewType = TransactionType.Expense;
        NewDate = DateTime.Now;
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
