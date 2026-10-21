using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using SaveMo.Models;
using SaveMo.Services.Interfaces;

namespace SaveMo.ViewModels;

public class PrePurchaseAnalyzerViewModel : INotifyPropertyChanged
{
    private readonly IReportService _reportService;

    private decimal _itemCost;
    private int _monthsToAnalyze = 12;
    private decimal _annualReturnRate = 7m;
    private PrePurchaseAnalysis? _analysisResult;
    private bool _hasAnalyzed;

    public decimal ItemCost
    {
        get => _itemCost;
        set => SetProperty(ref _itemCost, value);
    }

    public int MonthsToAnalyze
    {
        get => _monthsToAnalyze;
        set => SetProperty(ref _monthsToAnalyze, value);
    }

    public decimal AnnualReturnRate
    {
        get => _annualReturnRate;
        set => SetProperty(ref _annualReturnRate, value);
    }

    public PrePurchaseAnalysis? AnalysisResult
    {
        get => _analysisResult;
        set => SetProperty(ref _analysisResult, value);
    }

    public bool HasAnalyzed
    {
        get => _hasAnalyzed;
        set => SetProperty(ref _hasAnalyzed, value);
    }

    public ICommand AnalyzeCommand { get; }
    public ICommand ClearCommand { get; }

    public PrePurchaseAnalyzerViewModel(IReportService reportService)
    {
        _reportService = reportService;

        // Default values
        MonthsToAnalyze = 12;
        AnnualReturnRate = 5; // 5% annual return

        AnalyzeCommand = new Command(async () => await AnalyzeAsync());
        ClearCommand = new Command(() => Clear());
    }

    public async Task AnalyzeAsync()
    {
        if (ItemCost <= 0)
        {
            await Application.Current.MainPage.DisplayAlert("Validation Error", "Please enter a valid item cost.", "OK");
            return;
        }

        if (MonthsToAnalyze <= 0)
        {
            await Application.Current.MainPage.DisplayAlert("Validation Error", "Please enter valid number of months.", "OK");
            return;
        }

        try
        {
            AnalysisResult = await _reportService.AnalyzePurchaseAsync(ItemCost, MonthsToAnalyze, AnnualReturnRate);
            HasAnalyzed = true;
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Analysis failed: {ex.Message}", "OK");
        }
    }

    public void Clear()
    {
        ItemCost = 0;
        MonthsToAnalyze = 12;
        AnnualReturnRate = 5;
        AnalysisResult = null;
        HasAnalyzed = false;
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
