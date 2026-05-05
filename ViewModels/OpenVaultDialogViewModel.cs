using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KeyVaultHelper.Models;
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

    [ObservableProperty]
    public partial string? ResourceLoadError { get; set; }

    public OpenVaultDialogViewModel(AzureResourceCache indexService)
    {
        _indexService = indexService;

        Subscriptions = [];
        FilteredResourceGroups = [];
        FilteredVaults = [];
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
