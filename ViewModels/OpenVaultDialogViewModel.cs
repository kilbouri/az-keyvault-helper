using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KeyVaultHelper.Models.Data;
using KeyVaultHelper.Models.State;
using KeyVaultHelper.Services;

namespace KeyVaultHelper.ViewModels;

/// <summary>
/// ViewModel for the Open Vault dialog modal.
/// Manages subscription, resource group, and vault selection with cascading dropdowns.
/// </summary>
public partial class OpenVaultDialogViewModel(AzureResourceCache indexService, ResourceLoadingState loadingState) : ViewModelBase
{
    private readonly AzureResourceCache _indexService = indexService;
    private readonly ResourceLoadingState _loadingState = loadingState;

    [ObservableProperty]
    public partial Subscription? SelectedSubscription { get; set; }

    [ObservableProperty]
    public partial bool HasSelectedSubscription { get; set; }

    [ObservableProperty]
    public partial ResourceGroup? SelectedResourceGroup { get; set; }

    [ObservableProperty]
    public partial bool HasSelectedResourceGroup { get; set; }

    [ObservableProperty]
    public partial KeyVault? SelectedVault { get; set; }

    [ObservableProperty]
    public partial bool HasSelectedVault { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<Subscription> Subscriptions { get; set; } = [];

    [ObservableProperty]
    public partial ObservableCollection<ResourceGroup> FilteredResourceGroups { get; set; } = [];

    [ObservableProperty]
    public partial ObservableCollection<KeyVault> FilteredVaults { get; set; } = [];

    [ObservableProperty]
    public partial bool IsLoadingResources { get; set; }

    [ObservableProperty]
    public partial string? LoadingMessage { get; set; }

    [ObservableProperty]
    public partial string? ResourceLoadError { get; set; }

    partial void OnSelectedSubscriptionChanged(Subscription? value)
    {
        HasSelectedSubscription = value is not null;

        SelectedResourceGroup = null;
        HasSelectedResourceGroup = false;
        FilteredResourceGroups.Clear();

        SelectedVault = null;
        HasSelectedVault = false;
        FilteredVaults.Clear();

        if (value is not null)
        {
            foreach (var rg in _indexService.GetResourceGroups(value.Id))
            {
                FilteredResourceGroups.Add(rg);
            }
        }
    }

    partial void OnSelectedResourceGroupChanged(ResourceGroup? value)
    {
        HasSelectedResourceGroup = value is not null;

        SelectedVault = null;
        HasSelectedVault = false;
        FilteredVaults.Clear();

        if (value is not null)
        {
            foreach (var kv in _indexService.GetKeyVaults(value.Subscription.Id, value.Name))
            {
                FilteredVaults.Add(kv);
            }
        }
    }

    partial void OnSelectedVaultChanged(KeyVault? value)
    {
        HasSelectedVault = value is not null;
    }

    /// <summary>
    /// Initializes the dialog by waiting for background resource loading to complete
    /// and populating the subscriptions dropdown. Called when the dialog opens.
    /// </summary>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        // If cache already has subscriptions, just populate and return
        var cachedSubs = _indexService.GetSubscriptions().ToList();
        if (cachedSubs.Count > 0)
        {
            foreach (var sub in cachedSubs)
                Subscriptions.Add(sub);
            ResourceLoadError = null;
            return;
        }

        // If loading is in progress or there was an error, show loading state
        if (_loadingState.IsLoading || _loadingState.LoadError is not null)
        {
            IsLoadingResources = true;
            LoadingMessage = _loadingState.CurrentSubscriptionName ?? "Starting...";
        }

        // Wait for background load to complete or error
        while (_loadingState.IsLoading && !cancellationToken.IsCancellationRequested)
        {
            LoadingMessage = _loadingState.CurrentSubscriptionName ?? "Loading subscriptions...";
            await Task.Delay(100, cancellationToken);
        }

        // Check if we were cancelled
        if (cancellationToken.IsCancellationRequested)
        {
            IsLoadingResources = false;
            return;
        }

        // Check for errors
        if (_loadingState.LoadError is not null)
        {
            ResourceLoadError = _loadingState.LoadError;
            IsLoadingResources = false;
            LoadingMessage = null;
            return;
        }

        // Populate subscriptions from cache
        foreach (var sub in _indexService.GetSubscriptions())
            Subscriptions.Add(sub);

        IsLoadingResources = false;
        LoadingMessage = null;
        ResourceLoadError = null;
    }

    /// <summary>
    /// Retries loading resources from Azure. Called when user clicks the Retry button.
    /// </summary>
    [RelayCommand]
    public async Task RetryLoadResourcesAsync()
    {
        ResourceLoadError = null;
        IsLoadingResources = true;
        LoadingMessage = "Loading subscriptions...";

        try
        {
            var progress = new Progress<Subscription>(sub =>
            {
                LoadingMessage = $"Loading {sub.Name}...";
            });
            await _indexService.ReloadSubscriptionsAsync(progress);

            Subscriptions.Clear();
            foreach (var sub in _indexService.GetSubscriptions())
                Subscriptions.Add(sub);

            ResourceLoadError = null;
        }
        catch (Exception ex)
        {
            ResourceLoadError = $"Failed to load subscriptions: {ex.Message}";
        }
        finally
        {
            IsLoadingResources = false;
            LoadingMessage = null;
        }
    }
}
