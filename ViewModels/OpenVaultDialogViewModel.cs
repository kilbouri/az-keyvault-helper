using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using KeyVaultHelper.Models.Data;
using KeyVaultHelper.Services;

namespace KeyVaultHelper.ViewModels;

/// <summary>
/// ViewModel for the Open Vault dialog modal.
/// Manages subscription, resource group, and vault selection with cascading dropdowns.
/// </summary>
public partial class OpenVaultDialogViewModel : ViewModelBase
{
    private readonly AzureResourceCache _indexService;

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
    public partial ObservableCollection<Subscription> Subscriptions { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<ResourceGroup> FilteredResourceGroups { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<KeyVault> FilteredVaults { get; set; }

    [ObservableProperty]
    public partial bool IsLoadingResources { get; set; }

    [ObservableProperty]
    public partial string? LoadingMessage { get; set; }

    public OpenVaultDialogViewModel(AzureResourceCache indexService)
    {
        _indexService = indexService;

        Subscriptions = [];
        FilteredResourceGroups = [];
        FilteredVaults = [];

        // Bind cache loading states to this viewmodel's observable properties
        HandleCacheRefreshPhaseChange(_indexService.RefreshPhase);
        _indexService.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(AzureResourceCache.RefreshPhase))
            {
                HandleCacheRefreshPhaseChange(_indexService.RefreshPhase);
            }
        };
    }

    private void PopulateSubscriptionsDropdown()
    {
        Subscriptions.Clear();
        foreach (var sub in _indexService.GetSubscriptions())
        {
            Subscriptions.Add(sub);
        }
        Console.WriteLine($"Populated subscriptions dropdown with {Subscriptions.Count} subscriptions");
    }

    private void HandleCacheRefreshPhaseChange(AzureResourceCache.CacheRefreshPhase? newPhase)
    {
        IsLoadingResources = newPhase is not null;
        LoadingMessage = newPhase switch
        {
            AzureResourceCache.ListSubscriptionsPhase => "Loading subscriptions...",
            AzureResourceCache.ListResourcesInSubscriptionPhase listSubPhase => $"Loading resource groups and vaults in {listSubPhase.Subscription.Name}",
            null => null,
            _ => throw new NotImplementedException($"Unexpected cache refresh phase: {newPhase.GetType()}")
        };

        if (!IsLoadingResources)
        {
            PopulateSubscriptionsDropdown();
        }
    }

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
}
