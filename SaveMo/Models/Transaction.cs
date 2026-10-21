using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SaveMo.Models;

/// <summary>
/// Represents a financial transaction (income or expense).
/// Implements INotifyPropertyChanged for MVVM bindings.
/// </summary>
public class Transaction : INotifyPropertyChanged
{
    private int _id;
    private decimal _amount;
    private TransactionType _type;
    private string _category;
    private DateTime _date;
    private string _note;

    public int Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    public decimal Amount
    {
        get => _amount;
        set => SetProperty(ref _amount, value);
    }

    public TransactionType Type
    {
        get => _type;
        set => SetProperty(ref _type, value);
    }

    public string Category
    {
        get => _category;
        set => SetProperty(ref _category, value ?? string.Empty);
    }

    public DateTime Date
    {
        get => _date;
        set => SetProperty(ref _date, value);
    }

    public string Note
    {
        get => _note;
        set => SetProperty(ref _note, value ?? string.Empty);
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

public enum TransactionType
{
    Income,
    Expense
}
