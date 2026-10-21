using System.Diagnostics;

namespace SaveMo.Views;

/// <summary>
/// Base class for content pages that need async data loading.
/// Provides task tracking and cancellation token management to prevent:
/// - Multiple concurrent load operations
/// - Navigation blocking animation
/// - Race conditions with rapid page switching
/// </summary>
public abstract class BasePage : ContentPage
{
    /// <summary>
    /// Tracks the current load operation to prevent overlapping loads
    /// </summary>
    private CancellationTokenSource? _loadCancellation;
    private Task? _currentLoadTask;

    public BasePage()
    {
        // Subscribe to Unloaded event for cleanup
        Unloaded += BasePage_Unloaded;
    }

    /// <summary>
    /// Called when OnAppearing fires. Load cached data synchronously here
    /// to show immediately without blocking animation.
    /// </summary>
    /// <remarks>
    /// Override this to load cached/stale data quickly.
    /// This runs synchronously on the UI thread during animation,
    /// so keep it fast (just return existing collections).
    /// </remarks>
    protected virtual void LoadCachedData()
    {
        // Override in derived classes to load cached data
    }

    /// <summary>
    /// Called after LoadCachedData to refresh with fresh data asynchronously.
    /// Runs on background thread, doesn't block page animation.
    /// </summary>
    /// <remarks>
    /// Override this to load fresh data from repositories.
    /// Runs asynchronously so animation isn't blocked.
    /// </remarks>
    protected virtual async Task RefreshDataAsync(CancellationToken cancellationToken)
    {
        // Override in derived classes to load fresh data
        await Task.CompletedTask;
    }

    /// <summary>
    /// Properly handle OnAppearing with task tracking and cancellation
    /// </summary>
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            // Cancel any previous load operation
            _loadCancellation?.Cancel();
            _loadCancellation?.Dispose();

            // Create new cancellation token for this load
            _loadCancellation = new CancellationTokenSource();
            var cancellationToken = _loadCancellation.Token;

            // 1. Load cached data SYNCHRONOUSLY (fast, doesn't block animation)
            LoadCachedData();

            // 2. Then load fresh data ASYNCHRONOUSLY (doesn't block animation)
            _currentLoadTask = RefreshDataAsync(cancellationToken);
            await _currentLoadTask;
        }
        catch (OperationCanceledException)
        {
            // Load was cancelled because user navigated away - this is expected
            Debug.WriteLine($"Load cancelled for {GetType().Name}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading data for {GetType().Name}: {ex.Message}");
            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to load data: {ex.Message}", "OK");
            }
        }
    }

    /// <summary>
    /// Cancel ongoing load operations when navigating away
    /// </summary>
    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        // Cancel the load operation
        _loadCancellation?.Cancel();
    }

    /// <summary>
    /// Cleanup resources when page is unloaded
    /// </summary>
    private void BasePage_Unloaded(object? sender, EventArgs e)
    {
        _loadCancellation?.Dispose();
        _loadCancellation = null;
    }
}
