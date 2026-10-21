using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SaveMo.Models;

/// <summary>
/// Represents a monthly budget limit for a category with current spending tracking.
/// Implements INotifyPropertyChanged for MVVM bindings.
/// </summary>
public class Budget : INotifyPropertyChanged
{
    private int _id;
    private string _category;
    private decimal _monthlyLimit;
    private decimal _currentSpend;
    private int _month;
    private int _year;

    public int Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    public string Category
    {
        get => _category;
        set => SetProperty(ref _category, value ?? string.Empty);
    }

    public decimal MonthlyLimit
    {
        get => _monthlyLimit;
        set => SetProperty(ref _monthlyLimit, value);
    }

    public decimal CurrentSpend
    {
        get => _currentSpend;
        set => SetProperty(ref _currentSpend, value);
    }

    public int Month
    {
        get => _month;
        set => SetProperty(ref _month, value);
    }

    public int Year
    {
        get => _year;
        set => SetProperty(ref _year, value);
    }

    /// <summary>
    /// Calculates progress as a percentage (0-100).
    /// </summary>
    public double Progress
    {
        get
        {
            if (MonthlyLimit <= 0) return 0;
            return Math.Min(100, (double)(CurrentSpend / MonthlyLimit) * 100);
        }
    }

    /// <summary>
    /// Returns budget color state: Green (under), Yellow (near), Red (over).
    /// </summary>
    public string ProgressColor
    {
        get
        {
            var percentage = Progress;
            if (percentage >= 100) return "#FF6B6B"; // Red - over budget
            if (percentage >= 80) return "#FFD93D"; // Yellow - near budget
            return "#6BCB77"; // Green - under budget
        }
    }

    /// <summary>
    /// Returns remaining budget.
    /// </summary>
    public decimal Remaining => MonthlyLimit - CurrentSpend;

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
        // Notify dependent properties
        if (propertyName == nameof(CurrentSpend) || propertyName == nameof(MonthlyLimit))
        {
            OnPropertyChanged(nameof(Progress));
            OnPropertyChanged(nameof(ProgressColor));
            OnPropertyChanged(nameof(Remaining));
        }
        return true;
    }
}
