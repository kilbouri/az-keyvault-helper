using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
    private readonly NotificationService _notificationService;

    [ObservableProperty]
    public partial Subscription? SelectedSubscription { get; set; }

    [ObservableProperty]
    public partial ResourceGroup? SelectedResourceGroup { get; set; }

    [ObservableProperty]
    public partial KeyVault? SelectedVault { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<Subscription> Subscriptions { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<ResourceGroup> FilteredResourceGroups { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<KeyVault> FilteredVaults { get; set; }

    [ObservableProperty]
    public partial bool IsResourceGroupsLoading { get; set; }

    [ObservableProperty]
    public partial bool IsVaultsLoading { get; set; }

    [ObservableProperty]
    public partial bool IsOpenEnabled { get; set; }

    public IAsyncRelayCommand RefreshSubscriptionsCommand { get; }
    public IAsyncRelayCommand RefreshResourceGroupsCommand { get; }
    public IAsyncRelayCommand RefreshVaultsCommand { get; }

    public OpenVaultDialogViewModel(AzureResourceCache indexService, NotificationService notificationService)
    {
        _indexService = indexService;
        _notificationService = notificationService;

        Subscriptions = [];
        FilteredResourceGroups = [];
        FilteredVaults = [];

        RefreshSubscriptionsCommand = new AsyncRelayCommand(RefreshSubscriptionsAsync);

        // Populate subscriptions from index
        RefreshSubscriptionsDisplay();
    }

    private void RefreshSubscriptionsDisplay()
    {
        Subscriptions.Clear();
        foreach (var sub in _indexService.GetSubscriptions())
        {
            Subscriptions.Add(sub);
        }
    }

    partial void OnSelectedSubscriptionChanged(Subscription? oldValue, Subscription? newValue)
    {
        // Reset dependent selections
        SelectedResourceGroup = null;
        SelectedVault = null;
        FilteredVaults.Clear();

        if (newValue == null)
        {
            FilteredResourceGroups.Clear();
            return;
        }

        // Update filtered resource groups
        var resourceGroups = _indexService.GetResourceGroups(newValue.Id);
        FilteredResourceGroups.Clear();
        foreach (var rg in resourceGroups)
        {
            FilteredResourceGroups.Add(rg);
        }

        // Update loading state for resource groups
        OnPropertyChanged(nameof(IsResourceGroupsLoading));
    }

    partial void OnSelectedResourceGroupChanged(ResourceGroup? oldValue, ResourceGroup? newValue)
    {
        // Reset vault selection
        SelectedVault = null;

        if (newValue == null)
        {
            FilteredVaults.Clear();
            return;
        }

        if (SelectedSubscription == null)
            return;

        // Update filtered vaults
        var vaults = _indexService.GetKeyVaults(SelectedSubscription.Id, newValue.Name);
        FilteredVaults.Clear();
        foreach (var vault in vaults)
        {
            FilteredVaults.Add(vault);
        }

        // Update loading state for vaults
        OnPropertyChanged(nameof(IsVaultsLoading));
    }

    partial void OnSelectedVaultChanged(KeyVault? oldValue, KeyVault? newValue)
    {
        UpdateOpenButtonState();
    }

    private void UpdateOpenButtonState()
    {
        IsOpenEnabled = SelectedVault != null;
    }

    private async Task RefreshSubscriptionsAsync()
    {
        try
        {
            await _indexService.ReloadSubscriptionsAsync();
            RefreshSubscriptionsDisplay();
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Failed to refresh subscriptions: {ex.Message}");
        }
    }
}
