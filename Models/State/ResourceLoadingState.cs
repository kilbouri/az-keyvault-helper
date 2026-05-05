using CommunityToolkit.Mvvm.ComponentModel;

namespace KeyVaultHelper.Models.State;

/// <summary>
/// Tracks the app-wide resource loading state, including subscriptions being loaded and any errors.
/// </summary>
public partial class ResourceLoadingState : ObservableObject
{
    /// <summary>
    /// Indicates whether there is an ongoing data loading operation.
    /// </summary>
    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    /// <summary>
    /// Indicates the name of the subscription currently being loaded.
    /// </summary>
    [ObservableProperty]
    public partial string? CurrentSubscriptionName { get; set; }

    /// <summary>
    /// An error message indicating what went wrong loading data.
    /// </summary>
    [ObservableProperty]
    public partial string? LoadError { get; set; }

    /// <summary>
    /// Provides the ability to cancel an ongoing data loading operation.
    /// </summary>
    [ObservableProperty]
    public partial CancellationTokenSource? ActiveLoadCancellation { get; set; }
}
